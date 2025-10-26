import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { ClientInfo } from '../types/client';
// import axios from 'axios';

export interface ClientState {
  data: ClientInfo | null;
  saving: boolean;
  error: string | null;
}

const initialState: ClientState = {
  data: null,
  saving: false,
  error: null,
};

// return response.data as ClientInfo;
export const updateClientProfile = createAsyncThunk<
  ClientInfo,
  Partial<ClientInfo>,
  { rejectValue: string }
>(
  'client/updateProfile',
  async (patch, { getState, rejectWithValue }) => {
    try {
      const state: any = getState();
      const existing: ClientInfo | null = state.client?.data;
      if (!existing) {
        return rejectWithValue('Нет загруженных данных клиента');
      }

      const merged: ClientInfo = { ...existing, ...patch };

      await new Promise(r => setTimeout(r, 500));

      // const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7057';
      // const res = await axios.put(`${apiBase}/account/profile`, {
      //   clientId: merged.clientId,
      //   email: merged.email,
      //   phoneNumber: merged.phoneNumber,
      //   birthDate: merged.birthDate?.toISOString(),
      //   genderId: merged.genderId,
      // });
      // return res.data as ClientInfo;

      return merged;
    } catch (e: any) {
      return rejectWithValue(e?.message || 'Ошибка сохранения профиля');
    }
  }
);

const clientSlice = createSlice({
  name: 'client',
  initialState,
  reducers: {
    setClient(state, action: PayloadAction<ClientInfo>) {
      state.data = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(updateClientProfile.pending, (state) => {
        state.saving = true;
        state.error = null;
      })
      .addCase(updateClientProfile.fulfilled, (state, action) => {
        state.saving = false;
        state.data = action.payload;
      })
      .addCase(updateClientProfile.rejected, (state, action) => {
        state.saving = false;
        state.error = action.payload || 'Неизвестная ошибка';
      });
  },
});

export const { setClient } = clientSlice.actions;
export default clientSlice.reducer;
