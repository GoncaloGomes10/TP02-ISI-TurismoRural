import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './casas.module.css';

const Casas = () => {
  const [casas, setCasas] = useState([]);
  const [imagens, setImagens] = useState({}); 
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [user, setUser] = useState(null);
  const navigate = useNavigate();

  // Carregar perfil (para saber se é Support)
  useEffect(() => {
    const loadProfile = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        if (!token) return;

        const resp = await fetch('http://localhost:5211/api/Utilizadores/profile', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (resp.ok) {
          const data = await resp.json();
          setUser(data);
        }
      } catch (e) {
        console.error('Erro ao carregar perfil:', e);
      }
    };

    loadProfile();
  }, []);

  // Carregar casas + imagens
  useEffect(() => {
    const fetchCasas = async () => {
      try {
        const response = await fetch('http://localhost:5211/api/Casas/Casas');
        if (response.ok) {
          const data = await response.json();
          setCasas(data);
          await carregarImagens(data);
        } else {
          setError('Erro ao carregar casas.');
        }
      } catch (err) {
        console.error('Erro ao carregar casas:', err);
        setError('Erro de conexão. Tente novamente.');
      } finally {
        setIsLoading(false);
      }
    };

    const carregarImagens = async (listaCasas) => {
      const mapa = {};
      try {
        await Promise.all(
          listaCasas.map(async (c) => {
            const id = c.casaID || c.CasaID;
            try {
              const resp = await fetch(`http://localhost:5211/api/Casa_Img/${id}`);
              if (resp.ok) {
                const imgs = await resp.json();
                if (imgs && imgs.length > 0) {
                  const img = imgs[0];
                  mapa[id] =
                    img.url ||
                    img.Url ||
                    img.pathImagem ||
                    img.PathImagem;
                }
              }
            } catch (e) {
              console.error('Erro ao carregar imagem da casa', id, e);
            }
          })
        );
        setImagens(mapa);
      } catch (e) {
        console.error('Erro geral ao carregar imagens:', e);
      }
    };

    fetchCasas();
  }, []);

  const handleLogout = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      if (token) {
        await fetch('http://localhost:5211/api/Utilizadores/logout', {
          method: 'POST',
          headers: { 'Authorization': `Bearer ${token}` }
        });
      }
    } catch (error) {
      console.error('Erro no logout:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      navigate('/');
    }
  };

  const goHome = () => navigate('/');
  const goAdminCasas = () => navigate('/casas-admin');

  const isSupport = user?.isSupport === true || user?.IsSupport === true;

  if (isLoading) {
    return (
      <div className={styles.container}>
        <header className={styles.header}>
          <div className={styles.headerContent}>
            <div
              className={styles.logo}
              onClick={goHome}
              style={{ cursor: 'pointer' }}
            >
              <div className={styles.logoIcon}>🏕️</div>
              <div className={styles.logoText}>Turismo Rural</div>
            </div>
            <button className={styles.backButton} onClick={goHome}>
              Voltar
            </button>
          </div>
        </header>
        <main className={styles.main}>
          <p>A carregar casas...</p>
        </main>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <div
            className={styles.logo}
            onClick={goHome}
            style={{ cursor: 'pointer' }}
          >
            <div className={styles.logoIcon}>🏕️</div>
            <div className={styles.logoText}>Turismo Rural</div>
          </div>

          <div>
            {isSupport && (
              <button
                className={styles.backButton}
                onClick={goAdminCasas}
                style={{ marginRight: '0.75rem' }}
              >
                Gerir casas
              </button>
            )}
            <button
              className={styles.backButton}
              onClick={goHome}
              style={{ marginRight: '0.75rem' }}
            >
              Voltar
            </button>
            <button
              className={styles.backButton}
              onClick={handleLogout}
            >
              Sair
            </button>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className={styles.main}>
        <h1 className={styles.title}>Casas disponíveis</h1>
        <p className={styles.subtitle}>
          Escolha entre as nossas opções de alojamento rural.
        </p>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.grid}>
          {casas.map((casa) => {
            const id = casa.casaID || casa.CasaID;

            return (
              <div
                key={id}
                className={styles.card}
                onClick={() => navigate(`/casas/${id}`)}
                style={{ cursor: 'pointer' }}
              >
                {imagens[id] && (
                  <img
                    src={imagens[id]}
                    alt={casa.titulo || casa.Titulo}
                    className={styles.cardImage}
                  />
                )}

                <h2 className={styles.cardTitle}>{casa.titulo || casa.Titulo}</h2>

                <p className={styles.cardType}>
                  {(casa.tipo || casa.Tipo) || ''} · {(casa.tipologia || casa.Tipologia) || ''}
                </p>

                <p className={styles.cardDesc}>
                  {casa.descricao || casa.Descricao}
                </p>

                <p className={styles.cardPrice}>
                  {(casa.preco || casa.Preco)} € / noite
                </p>

                <p className={styles.cardAddress}>
                  {(casa.morada || casa.Morada)}, {(casa.codigoPostal || casa.CodigoPostal)}
                </p>
              </div>
            );
          })}
        </div>
      </main>

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

export default Casas;
