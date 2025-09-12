import { jwtDecode } from "jwt-decode";

export const getToken = () => localStorage.getItem("access_token");

export const removeToken = () => localStorage.removeItem("access_token");

export const isTokenValid = (token) => {
  try {
    const { exp } = jwtDecode(token);
    return Date.now() < exp * 1000;
  } catch {
    return false;
  }
};

export const checkAndCleanToken = () => {
  const token = getToken();
  if (!token || !isTokenValid(token)) {
    removeToken();
    return false;
  }
  return true;
};

export const isAuthenticated = () => {
  const token = getToken();
  return token && isTokenValid(token);
};

export const getUserIdFromToken = () => {
  const token = getToken();
  if (!token) return null;
  try {
    const decoded = jwtDecode(token);
    return decoded.nameid || null;
  } catch {
    return null;
  }
};

export const getUserEmailFromToken = () => {
  const token = getToken();
  if (!token) return null;
  try {
    const decoded = jwtDecode(token);
    return decoded.email || null;
  } catch {
    return null;
  }
};