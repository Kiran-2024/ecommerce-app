import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  sub: string;
  email: string;
  role: string;
  rights: string | string[];
  exp: number;
  nameid: string;
}

export class TokenHelper {

  static decodeToken(token: string): JwtPayload | null {
    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  }

  static getRole(token: string): string | null {
    const decoded = this.decodeToken(token);
    return decoded?.role ?? null;
  }

  static getRights(token: string): string[] {
    const decoded = this.decodeToken(token);
    if (!decoded?.rights) return [];
    return Array.isArray(decoded.rights) ? decoded.rights : [decoded.rights];
  }

  static getUserId(token: string): string | null {
    const decoded = this.decodeToken(token);
    return decoded?.nameid ?? null;
  }

  static getEmail(token: string): string | null {
    const decoded = this.decodeToken(token);
    return decoded?.email ?? null;
  }

  static isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded?.exp) return true;
    return decoded.exp * 1000 < Date.now();
  }
}