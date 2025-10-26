export interface ClientInfo {
  clientId: number;
  firstName: string;
  lastName: string;
  middleName?: string;
  email: string;
  emailConfirmed?: boolean;
  genderId?: number;
  birthDate?: Date;
  phoneNumber?: string;
  phoneNumberConfirmed?: boolean | null;
  accessCardNumber?: string;
  photo?: string;
  createdAt?: string;
}
