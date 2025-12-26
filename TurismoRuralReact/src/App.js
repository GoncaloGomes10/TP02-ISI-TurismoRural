import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';

import WelcomePage from './Welcomepage/WelcomePage.js';
import Login from './Login/login.js';
import Signup from './Signup/signup.js';
import Perfil from './Perfil/perfil.js';
import Casas from './Casas/casas.js';
import CasaDetalhes from './Casas/casadetalhes.js';
import MinhasReservas from './MinhasReservas/minhasreservas.js';
import Utilizadores from './Utilizadores/utilizadores.js';
import CasasAdmin from './Casas/casaadmin.js';
import { PrivateRoute, AdminRoute } from './Routes/PrivateRoute.js';

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          {/* Pública */}
          <Route path="/" element={<WelcomePage />} />

          {/* Casas: só logado pode reservar / ver detalhes */}
          <Route
            path="/casas"
            element={
              <PrivateRoute>
                <Casas />
              </PrivateRoute>
            }
          />
          <Route
            path="/casas/:id"
            element={
              <PrivateRoute>
                <CasaDetalhes />
              </PrivateRoute>
            }
          />
          <Route
            path="/minhas-reservas"
            element={
              <PrivateRoute>
                <MinhasReservas />
              </PrivateRoute>
            }
          />

          {/* Apenas admin/support */}
          <Route
            path="/casas-admin"
            element={
              <AdminRoute>
                <CasasAdmin />
              </AdminRoute>
            }
          />
          <Route
            path="/utilizadores"
            element={
              <AdminRoute>
                <Utilizadores />
              </AdminRoute>
            }
          />

          {/* Autenticação */}
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<Signup />} />

          {/* Perfil (user logado) */}
          <Route
            path="/perfil"
            element={
              <PrivateRoute>
                <Perfil />
              </PrivateRoute>
            }
          />

          {/* fallback */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
