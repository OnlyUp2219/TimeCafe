import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type {RootState} from "@store";
import { Gender, ProfileStatus, type Profile } from '@app-types/profile';
import { ProfileApi } from '@api/profile/profileApi';
import {clearTokens} from "@store/authSlice";

export interface ProfileState {
  data: Profile | null;
  loading: boolean;
  saving: boolean;
  error: string | null;
  loadedUserId: string | null;
}

const createEmptyProfile = (): Profile => ({
  firstName: '',
  lastName: '',
  middleName: undefined,

  email: undefined,
  emailConfirmed: false,
  phoneNumber: undefined,
  phoneNumberConfirmed: null,

  gender: Gender.NotSpecified,
  birthDate: undefined,
  photoUrl: undefined,

  profileStatus: ProfileStatus.Pending,
  banReason: undefined,
});

const getStatusCode = (error: unknown): number | null => {
  const anyError = error as { response?: { status?: unknown }; statusCode?: unknown };

  const statusFromAxios = anyError?.response?.status;
  if (typeof statusFromAxios === 'number') return statusFromAxios;

  const statusFromApiError = anyError?.statusCode;
  if (typeof statusFromApiError === 'number') return statusFromApiError;

  return null;
};

const getErrorMessage = (error: unknown, fallback: string): string => {
  const anyError = error as { message?: unknown };
  const msg = anyError?.message;
  return typeof msg === 'string' && msg.trim() ? msg : fallback;
};

const initialState: ProfileState = {
  data: null,
  loading: false,
  saving: false,
  error: null,
  loadedUserId: null,
};

export const fetchProfileByUserId = createAsyncThunk<
  Profile,
  { userId: string },
  { rejectValue: string }
>('profile/fetchByUserId', async ({ userId }, { rejectWithValue }) => {
  try {
    return await ProfileApi.getProfileByUserId(userId);
  } catch (e: unknown) {
    const status = getStatusCode(e);
    if (status === 404) {
      try {
        await ProfileApi.createEmptyProfile(userId);
        return await ProfileApi.getProfileByUserId(userId);
      } catch (inner: unknown) {
        return rejectWithValue(getErrorMessage(inner, 'Ошибка загрузки профиля'));
      }
    }

    return rejectWithValue(getErrorMessage(e, 'Ошибка загрузки профиля'));
  }
});

const isUpdatableViaProfileApi = (patch: Partial<Profile>): boolean => {
  return (
    Object.prototype.hasOwnProperty.call(patch, 'firstName') ||
    Object.prototype.hasOwnProperty.call(patch, 'lastName') ||
    Object.prototype.hasOwnProperty.call(patch, 'middleName') ||
    Object.prototype.hasOwnProperty.call(patch, 'gender') ||
    Object.prototype.hasOwnProperty.call(patch, 'birthDate') ||
    Object.prototype.hasOwnProperty.call(patch, 'photoUrl')
  );
};

const normalizeBirthDateForApi = (value: string | undefined): string | null => {
  const trimmed = (value ?? '').trim();
  if (!trimmed) return null;

  const m = /^\d{4}-\d{2}-\d{2}/.exec(trimmed);
  if (!m) return null;
  return trimmed.slice(0, 10);
};

export const updateProfile = createAsyncThunk<
  Profile,
  Partial<Profile>,
  { state: RootState; rejectValue: string }
>('profile/update', async (patch, { getState, rejectWithValue }) => {
  try {
    const state = getState();
    const existing = state.profile?.data ?? createEmptyProfile();
    const sanitizedPatch: Partial<Profile> = { ...patch };
    const merged: Profile = { ...existing, ...sanitizedPatch };

    const userId = state.auth.userId;
    if (!userId) {
      return rejectWithValue('Не найден userId пользователя');
    }

    if (!isUpdatableViaProfileApi(sanitizedPatch)) {
      return merged;
    }

    const savedResponse = await ProfileApi.updateProfile({
      userId,
      firstName: merged.firstName ?? '',
      lastName: merged.lastName ?? '',
      middleName: merged.middleName ?? null,
      photoUrl: merged.photoUrl ?? null,
      birthDate: normalizeBirthDateForApi(merged.birthDate),
      gender: merged.gender ?? Gender.NotSpecified,
    });

    return { ...merged, ...savedResponse.profile };
  } catch (e: unknown) {
    const message = e instanceof Error ? e.message : 'Ошибка сохранения профиля';
    return rejectWithValue(message);
  }
});

const profileSlice = createSlice({
  name: 'profile',
  initialState,
  reducers: {
    setProfile(state, action: PayloadAction<Profile>) {
      state.data = action.payload;
      state.loadedUserId = null;
    },
    setProfileForUser(state, action: PayloadAction<{ profile: Profile; userId: string }>) {
      state.data = action.payload.profile;
      state.loadedUserId = action.payload.userId;
    },
    resetProfile(state) {
      state.data = null;
      state.loading = false;
      state.saving = false;
      state.error = null;
      state.loadedUserId = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase('persist/REHYDRATE' as never, (state) => {
        state.loading = false;
        state.saving = false;
        state.error = null;
      })
      .addCase(clearTokens, (state) => {
        state.data = null;
        state.loading = false;
        state.saving = false;
        state.error = null;
        state.loadedUserId = null;
      })
      .addCase(fetchProfileByUserId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProfileByUserId.fulfilled, (state, action) => {
        state.loading = false;
        state.data = action.payload;
        state.loadedUserId = action.meta.arg.userId;
      })
      .addCase(fetchProfileByUserId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || 'Неизвестная ошибка';
      })
      .addCase(updateProfile.pending, (state) => {
        state.saving = true;
        state.error = null;
      })
      .addCase(updateProfile.fulfilled, (state, action) => {
        state.saving = false;
        state.data = action.payload;
      })
      .addCase(updateProfile.rejected, (state, action) => {
        state.saving = false;
        state.error = action.payload || 'Неизвестная ошибка';
      });
  },
});

export const { setProfile, setProfileForUser, resetProfile } = profileSlice.actions;
export default profileSlice.reducer;
