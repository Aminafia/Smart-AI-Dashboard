import { JwtPayload } from 'jwt-decode';
import { JwtClaims } from '../../constants/jwt-claims';

export interface AppJwtPayload extends JwtPayload {
    [JwtClaims.UserId]: string;
    [JwtClaims.Email]: string;
    [JwtClaims.FullName]: string;
    [JwtClaims.Role]: string;
}