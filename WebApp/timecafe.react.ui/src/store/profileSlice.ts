import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from './index';
import { Gender, ProfileStatus, type Profile } from '../types/profile';
import { ProfileApi } from '../shared/api/profile/profileApi';

export interface ProfileState {
  data: Profile | null;
  loading: boolean;
  saving: boolean;
  error: string | null;
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

  accessCardNumber: undefined,
  photoUrl: undefined,

  profileStatus: ProfileStatus.Pending,
  banReason: undefined,
});

const getStatusCode = (error: unknown): number | null => {
  const anyError = error as { response?: { status?: unknown } };
  const status = anyError?.response?.status;
  return typeof status === 'number' ? status : null;
};

const initialState: ProfileState = {
  data: null,
  loading: false,
  saving: false,
  error: null,
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
        const message = inner instanceof Error ? inner.message : 'Ошибка загрузки профиля';
        return rejectWithValue(message);
      }
    }

    const message = e instanceof Error ? e.message : 'Ошибка загрузки профиля';
    return rejectWithValue(message);
  }
});

const isUpdatableViaProfileApi = (patch: Partial<Profile>): boolean => {
  return (
    Object.prototype.hasOwnProperty.call(patch, 'firstName') ||
    Object.prototype.hasOwnProperty.call(patch, 'lastName') ||
    Object.prototype.hasOwnProperty.call(patch, 'middleName') ||
    Object.prototype.hasOwnProperty.call(patch, 'gender') ||
    Object.prototype.hasOwnProperty.call(patch, 'birthDate') ||
    Object.prototype.hasOwnProperty.call(patch, 'photoUrl') ||
    Object.prototype.hasOwnProperty.call(patch, 'accessCardNumber')
  );
};

export const updateProfile = createAsyncThunk<
  Profile,
  Partial<Profile>,
  { state: RootState; rejectValue: string }
>('profile/update', async (patch, { getState, rejectWithValue }) => {
  try {
    const state = getState();
    const existing = state.profile?.data ?? createEmptyProfile();
    const merged: Profile = { ...existing, ...patch };

    const userId = state.auth.userId;
    if (!userId) {
      return rejectWithValue('Не найден userId пользователя');
    }

    if (!isUpdatableViaProfileApi(patch)) {
      return merged;
    }

    const saved = await ProfileApi.updateProfile({
      userId,
      firstName: merged.firstName ?? '',
      lastName: merged.lastName ?? '',
      middleName: merged.middleName ?? null,
      accessCardNumber: merged.accessCardNumber ?? null,
      photoUrl: merged.photoUrl ?? null,
      birthDate: merged.birthDate ?? null,
      gender: merged.gender ?? Gender.NotSpecified,
    });

    return { ...merged, ...saved };
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
    },
    resetProfile(state) {
      state.data = null;
      state.loading = false;
      state.saving = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase('persist/REHYDRATE' as never, (state) => {
        state.loading = false;
        state.saving = false;
        state.error = null;
      })
      .addCase(fetchProfileByUserId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProfileByUserId.fulfilled, (state, action) => {
        state.loading = false;
        state.data = action.payload;
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

export const { setProfile, resetProfile } = profileSlice.actions;
export default profileSlice.reducer;
