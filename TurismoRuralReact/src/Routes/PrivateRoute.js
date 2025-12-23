import React from 'react';
import { Navigate } from 'react-router-dom';

const isAuthenticated = () => {
  const token = localStorage.getItem('accessToken');
  return !!token;
};

export const PrivateRoute = ({ children }) => {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }
  return children;
};

export const AdminRoute = ({ children }) => {
  const token = localStorage.getItem('accessToken');
  if (!token) {
    return <Navigate to="/login" replace />;
  }

  // o role vem no payload do JWT; isto é uma verificação simples no cliente
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (role !== 'Support' && role !== 'Suport') {
      return <Navigate to="/" replace />;
    }
  } catch {
    return <Navigate to="/login" replace />;
  }

  return children;
};
