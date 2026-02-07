export const Gender = {
  NotSpecified: 0,
  Male: 1,
  Female: 2,
} as const;

export type Gender = (typeof Gender)[keyof typeof Gender];

export const ProfileStatus = {
  Pending: 0,
  Completed: 1,
  Banned: 2,
} as const;

export type ProfileStatus = (typeof ProfileStatus)[keyof typeof ProfileStatus];

export interface Profile {
  firstName: string;
  lastName: string;
  middleName?: string;

  email?: string;
  emailConfirmed?: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed?: boolean | null;

  gender: Gender;
  birthDate?: string;
  photoUrl?: string;

  profileStatus: ProfileStatus;
  banReason?: string;
}
