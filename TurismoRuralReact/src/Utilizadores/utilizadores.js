import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './utilizadores.module.css';   // novo CSS

const Utilizadores = () => {
  const navigate = useNavigate();
  const [utilizadores, setUtilizadores] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');
  const [apagandoId, setApagandoId] = useState(null);

  const carregarUtilizadores = async () => {
    setErro('');
    setMensagem('');
    try {
      const token = localStorage.getItem('accessToken');
      const resp = await fetch('http://localhost:5211/api/Utilizadores', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (resp.ok) {
        const data = await resp.json();
        setUtilizadores(data);
      } else {
        const text = await resp.text();
        setErro(text || 'Erro ao carregar utilizadores.');
      }
    } catch (e) {
      console.error('Erro ao carregar utilizadores:', e);
      setErro('Erro de conexão. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    carregarUtilizadores();
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    navigate('/');
  };

  const goHome = () => navigate('/');
  const goBack = () => navigate(-1);

  const apagarUtilizador = async (id) => {
    if (!window.confirm('Tem a certeza que pretende apagar este utilizador?')) return;

    setErro('');
    setMensagem('');
    setApagandoId(id);

    try {
      const token = localStorage.getItem('accessToken');
      const resp = await fetch(`http://localhost:5211/api/Utilizadores/delete/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (resp.ok) {
        const text = await resp.text();
        setMensagem(text || 'Utilizador removido com sucesso.');
        await carregarUtilizadores();
      } else {
        const text = await resp.text();
        setErro(text || 'Erro ao apagar utilizador.');
      }
    } catch (e) {
      console.error('Erro ao apagar utilizador:', e);
      setErro('Erro de conexão. Tente novamente.');
    } finally {
      setApagandoId(null);
    }
  };

  if (isLoading) {
    return (
      <div className={styles.container}>
        <main className={styles.main}>
          <p>A carregar utilizadores...</p>
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
          <button onClick={handleLogout} className={styles.logoutButton}>
            Sair
          </button>
        </div>
      </header>

      {/* Conteúdo */}
      <main className={styles.main}>
        <h1 className={styles.title}>Lista de Utilizadores</h1>

        {mensagem && <div className={styles.messageSuccess}>{mensagem}</div>}
        {erro && <div className={styles.error}>{erro}</div>}

        {utilizadores.length === 0 ? (
          <p className={styles.empty}>Ainda não existem utilizadores.</p>
        ) : (
          <div className={styles.tableWrapper}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Telemóvel</th>
                  <th>Admin</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {utilizadores.map((u) => (
                  <tr key={u.utilizadorID || u.UtilizadorID}>
                    <td>{u.utilizadorID || u.UtilizadorID}</td>
                    <td>{u.nome || u.Nome}</td>
                    <td>{u.email || u.Email}</td>
                    <td>{u.telemovel || u.Telemovel}</td>
                    <td>{(u.isSupport ?? u.IsSupport) ? 'Sim' : 'Não'}</td>
                    <td>
                      <button
                        className={styles.buttonDanger}
                        onClick={() => apagarUtilizador(u.utilizadorID || u.UtilizadorID)}
                        disabled={apagandoId === (u.utilizadorID || u.UtilizadorID)}
                      >
                        {apagandoId === (u.utilizadorID || u.UtilizadorID)
                          ? 'A apagar...'
                          : 'Apagar'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <button className={styles.backButton} onClick={goBack}>
          Voltar
        </button>
      </main>

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

export default Utilizadores;
