export enum Gender {
  NotSpecified = 0,
  Male = 1,
  Female = 2,
}

export enum ProfileStatus {
  Pending = 0,
  Completed = 1,
  Banned = 2,
}

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

  accessCardNumber?: string;
  photoUrl?: string;

  profileStatus: ProfileStatus;
  banReason?: string;
}
