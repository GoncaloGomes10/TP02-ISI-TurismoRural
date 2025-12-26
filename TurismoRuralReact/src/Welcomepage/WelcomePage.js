import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './WelcomePage.module.css';

const WelcomePage = () => {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  // Verificar se está autenticado e carregar perfil
  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    console.log('🔍 DEBUG - Iniciando checkAuth...');

    try {
      const token = localStorage.getItem('accessToken');
      console.log('🔍 TOKEN:', token ? 'EXISTS' : 'NULL');

      if (token) {
        console.log('🔍 Fazendo fetch para profile...');
        const response = await fetch('http://localhost:5211/api/Utilizadores/profile', {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        console.log('🔍 RESPONSE STATUS:', response.status);

        if (response.ok) {
          const profileData = await response.json();
          console.log('🔍 PERFIL:', profileData);
          setUser(profileData);
        } else {
          console.log('🔍 Token inválido, limpando...');
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      }
    } catch (error) {
      console.error('🔍 ERRO:', error);
    } finally {
      console.log('🔍 FINALIZANDO - setIsLoading(false)');
      setIsLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      if (token) {
        await fetch('http://localhost:5211/api/Utilizadores/logout', {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
      }
    } catch (error) {
      console.error('Erro no logout:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      setUser(null);
      navigate('/');
    }
  };

  const goToProfile = () => navigate('/perfil');
  const goToLogin = () => navigate('/login');
  const goToSignup = () => navigate('/signup');
  const goToCasas = () => navigate('/casas');
  const goToMinhasReservas = () => navigate('/minhas-reservas');
  const goToListaUtilizadores = () => navigate('/utilizadores'); 

  if (isLoading) {
    return (
      <div className={styles.container}>
        <div className={styles.heroSection}>
          <div style={{ textAlign: 'center', padding: '4rem' }}>
            A carregar...
          </div>
        </div>
      </div>
    );
  }

  
  const isAdmin = user?.isSupport === true || user?.IsSupport === true;

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <div 
            className={styles.logo}
            onClick={() => navigate('/')}
            style={{ cursor: 'pointer' }}
          >
            <div className={styles.logoIcon}>🏕️</div>
            <div className={styles.logoText}>Turismo Rural</div>
          </div>
          {user ? (
            <button onClick={handleLogout} className={styles.logoutButton}>
              Sair
            </button>
          ) : (
            <button onClick={goToLogin} className={styles.logoutButton}>
              Entrar
            </button>
          )}
        </div>
      </header>

      {/* Hero Section */}
      <section className={styles.heroSection}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Descubra o 
            <span className={styles.heroTitleAccent}>melhor do turismo rural</span>
          </h1>
          <p className={styles.heroDescription}>
            Encontre alojamentos únicos em aldeias autênticas, viva experiências 
            tradicionais e conecte-se com a natureza.
          </p>
          
          <div className={styles.buttonContainer}>
            {user ? (
              isAdmin ? (
                // ADMIN: Perfil + Ver casas + Lista de Utilizadores + Sair
                <>
                  <button 
                    onClick={goToProfile}
                    className={`${styles.button} ${styles.buttonPrimary}`}
                  >
                    Ir ao Perfil
                  </button>
                  <button 
                    onClick={goToCasas}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Ver casas disponíveis
                  </button>
                  <button 
                    onClick={goToListaUtilizadores}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Lista de Utilizadores
                  </button>
                  <button 
                    onClick={handleLogout}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Sair
                  </button>
                </>
              ) : (
                // USER normal: Perfil + Minhas reservas + Ver casas + Sair
                <>
                  <button 
                    onClick={goToProfile}
                    className={`${styles.button} ${styles.buttonPrimary}`}
                  >
                    Ir ao Perfil
                  </button>
                  <button 
                    onClick={goToMinhasReservas}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Minhas reservas
                  </button>
                  <button 
                    onClick={goToCasas}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Ver casas disponíveis
                  </button>
                  <button 
                    onClick={handleLogout}
                    className={`${styles.button} ${styles.buttonSecondary}`}
                  >
                    Sair
                  </button>
                </>
              )
            ) : (
              // visitante (não logado)
              <>
                <button 
                  onClick={goToSignup}
                  className={`${styles.button} ${styles.buttonPrimary}`}
                >
                  Começar Agora
                </button>
                <button 
                  onClick={goToLogin}
                  className={`${styles.button} ${styles.buttonSecondary}`}
                >
                  Já tenho conta
                </button>
              </>
            )}
          </div>
        </div>
      </section>

      {/* Features */}
      <section className={styles.featuresGrid}>
        <div className={styles.featureCard}>
          <div className={`${styles.featureIcon} ${styles.featureIconGreen}`}>
            🏡
          </div>
          <h3 className={styles.featureTitle}>Alojamentos Autênticos</h3>
          <p className={styles.featureDescription}>
            Casas tradicionais renovadas em aldeias pitorescas com toda 
            a comodidade moderna.
          </p>
        </div>

        <div className={styles.featureCard}>
          <div className={`${styles.featureIcon} ${styles.featureIconBlue}`}>
            🌿
          </div>
          <h3 className={styles.featureTitle}>Experiências Locais</h3>
          <p className={styles.featureDescription}>
            Provas de vinhos, caminhadas guiadas, recolha de ervas silvestres 
            e muito mais.
          </p>
        </div>

        <div className={styles.featureCard}>
          <div className={`${styles.featureIcon} ${styles.featureIconYellow}`}>
            ⭐
          </div>
          <h3 className={styles.featureTitle}>Verificação Completa</h3>
          <p className={styles.featureDescription}>
            Todos os anfitriões e alojamentos são verificados para garantir 
            a melhor experiência.
          </p>
        </div>
      </section>

      {/* Footer */}
      <footer className={styles.footer}>
        <div className={styles.footerContent}>
          <p className={styles.footerText}>
            © 2025 Turismo Rural. Descubra Portugal autêntico.
          </p>
        </div>
      </footer>
    </div>
  );
};

export default WelcomePage;
