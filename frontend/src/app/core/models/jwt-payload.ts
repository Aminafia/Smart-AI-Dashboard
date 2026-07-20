import { JwtClaims } from '../constants/jwt-claims';

export interface JwtPayload {
  [JwtClaims.UserId]: string;
  [JwtClaims.Email]: string;
  [JwtClaims.FullName]: string;
  [JwtClaims.Role]: string;
}