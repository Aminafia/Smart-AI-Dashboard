export interface CurrentUser {
  readonly userId: string;
  readonly email: string;
  readonly role: string;
  readonly fullName: string;
}