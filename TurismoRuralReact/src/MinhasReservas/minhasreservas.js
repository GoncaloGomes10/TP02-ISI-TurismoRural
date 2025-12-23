import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './minhasreservas.module.css';

const MinhasReservas = () => {
  const [reservas, setReservas] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [mensagem, setMensagem] = useState('');

  // edição de reserva
  const [editandoId, setEditandoId] = useState(null);
  const [editDataInicio, setEditDataInicio] = useState('');
  const [editDataFim, setEditDataFim] = useState('');

  // estados para avaliação
  const [avaliacaoNota, setAvaliacaoNota] = useState(0);
  const [avaliacaoComentario, setAvaliacaoComentario] = useState('');
  const [avaliacaoLoadingId, setAvaliacaoLoadingId] = useState(null);
  const navigate = useNavigate();

  const formatData = (d) => {
    if (!d) return '';
    return d.toString().split('T')[0];
  };

  const carregarReservas = async () => {
    try {
      const token = localStorage.getItem('accessToken');

      await fetch('http://localhost:5211/api/Reservas/AtualizarEstados', {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      const response = await fetch('http://localhost:5211/api/Reservas/MinhasReservas', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setReservas(data);
      } else {
        setError('Erro ao carregar reservas.');
      }
    } catch (err) {
      console.error('Erro ao carregar reservas do utilizador:', err);
      setError('Erro de conexão. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    carregarReservas();
  }, []);

  const goHome = () => navigate('/');
  const goBack = () => navigate(-1);

  // ----- RESERVA: editar / cancelar -----
  const iniciarEdicao = (reserva) => {
    setEditandoId(reserva.reservaID || reserva.ReservaID);
    setEditDataInicio(formatData(reserva.dataInicio || reserva.DataInicio));
    setEditDataFim(formatData(reserva.dataFim || reserva.DataFim));
    setMensagem('');
    setError('');
  };

  const cancelarEdicao = () => {
    setEditandoId(null);
    setEditDataInicio('');
    setEditDataFim('');
  };

  const salvarEdicao = async (id) => {
    setMensagem('');
    setError('');

    if (!editDataInicio || !editDataFim) {
      setError('Preencha as datas de início e fim.');
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');
      const body = {
        dataInicio: editDataInicio,
        dataFim: editDataFim
      };

      const response = await fetch(`http://localhost:5211/api/Reservas/EditarReserva/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      });

      if (response.ok) {
        const text = await response.text();
        setMensagem(text || 'Reserva editada com sucesso.');
        setEditandoId(null);
        await carregarReservas();
      } else {
        const text = await response.text();
        console.error('Erro ao editar reserva:', text);
        setError(text || 'Erro ao editar reserva.');
      }
    } catch (err) {
      console.error('Erro na edição:', err);
      setError('Erro de conexão. Tente novamente.');
    }
  };

  const cancelarReserva = async (id) => {
    setMensagem('');
    setError('');

    if (!window.confirm('Tem a certeza que pretende cancelar esta reserva?')) {
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`http://localhost:5211/api/Reservas/CancelarReserva/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const text = await response.text();
        setMensagem(text || 'Reserva cancelada com sucesso.');
        await carregarReservas();
      } else {
        const text = await response.text();
        console.error('Erro ao cancelar reserva:', text);
        setError(text || 'Erro ao cancelar reserva.');
      }
    } catch (err) {
      console.error('Erro no cancelamento:', err);
      setError('Erro de conexão. Tente novamente.');
    }
  };

  // ----- AVALIAÇÕES: criar / editar / apagar -----
  const iniciarAvaliacao = (reserva) => {
    setAvaliacaoNota(0);
    setAvaliacaoComentario('');
    setAvaliacaoLoadingId(reserva.casaID || reserva.CasaID);
    setMensagem('');
    setError('');
  };

  const iniciarEdicaoAvaliacao = (reserva) => {
    setAvaliacaoNota(reserva.avaliacaoNota || 0);
    setAvaliacaoComentario(reserva.avaliacaoComentario || '');
    setAvaliacaoLoadingId(reserva.casaID || reserva.CasaID);
    setMensagem('');
    setError('');
  };

  const cancelarAvaliacao = () => {
    setAvaliacaoLoadingId(null);
    setAvaliacaoNota(0);
    setAvaliacaoComentario('');
  };

  const handleCriarAvaliacao = async (casaId) => {
    setMensagem('');
    setError('');

    if (avaliacaoNota < 0 || avaliacaoNota > 5) {
      setError('A nota tem de ser entre 0 e 5.');
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');
      const body = {
        casaID: casaId,
        nota: avaliacaoNota,
        comentario: avaliacaoComentario
      };

      const resp = await fetch('http://localhost:5211/api/Avaliacoes/CriarAvaliacao', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      });

      if (resp.ok) {
        const text = await resp.text();
        setMensagem(text || 'Avaliação criada com sucesso!');
        cancelarAvaliacao();
        await carregarReservas();
      } else {
        const text = await resp.text();
        setError(text || 'Erro ao criar avaliação.');
      }
    } catch (err) {
      console.error('Erro ao criar avaliação:', err);
      setError('Erro de conexão. Tente novamente.');
    }
  };

  const handleEditarAvaliacao = async (avaliacaoId) => {
    setMensagem('');
    setError('');

    if (avaliacaoNota < 0 || avaliacaoNota > 5) {
      setError('A nota tem de ser entre 0 e 5.');
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');
      const body = {
        nota: avaliacaoNota,
        comentario: avaliacaoComentario
      };

      const resp = await fetch(`http://localhost:5211/api/Avaliacoes/EditarAvalicao/${avaliacaoId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      });

      if (resp.ok) {
        const text = await resp.text();
        setMensagem(text || 'Avaliação editada com sucesso!');
        cancelarAvaliacao();
        await carregarReservas();
      } else {
        const text = await resp.text();
        setError(text || 'Erro ao editar avaliação.');
      }
    } catch (err) {
      console.error('Erro ao editar avaliação:', err);
      setError('Erro de conexão. Tente novamente.');
    }
  };

  const handleApagarAvaliacao = async (avaliacaoId) => {
    setMensagem('');
    setError('');

    if (!window.confirm('Tem a certeza que pretende apagar esta avaliação?')) {
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');

      const resp = await fetch(`http://localhost:5211/api/Avaliacoes/ApagarAvalicao/${avaliacaoId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (resp.ok) {
        const text = await resp.text();
        setMensagem(text || 'Avaliação apagada com sucesso!');
        cancelarAvaliacao();
        await carregarReservas();
      } else {
        const text = await resp.text();
        setError(text || 'Erro ao apagar avaliação.');
      }
    } catch (err) {
      console.error('Erro ao apagar avaliação:', err);
      setError('Erro de conexão. Tente novamente.');
    }
  };

  if (isLoading) {
    return (
      <div className={styles.container}>
        <main className={styles.main}>
          <p>A carregar reservas...</p>
        </main>
      </div>
    );
  }

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
          <div>
            <button 
              className={styles.backButton} 
              onClick={goBack}
              style={{ marginRight: '0.75rem' }}
            >
              Voltar
            </button>
            <button 
              className={styles.backButton} 
              onClick={goHome}
            >
              Início
            </button>
          </div>
        </div>
      </header>

      <main className={styles.main}>
        <h1 className={styles.title}>Minhas reservas</h1>

        {mensagem && (
          <div className={styles.messageSuccess}>
            {mensagem}
          </div>
        )}

        {error && (
          <div className={styles.error}>
            {error}
          </div>
        )}

        {reservas.length === 0 ? (
          <p className={styles.reservasEmpty}>Ainda não tem reservas.</p>
        ) : (
          <div className={styles.grid}>
            {reservas.map((r) => {
              const id = r.reservaID || r.ReservaID;
              const estado = r.estado || r.Estado;
              const dataIni = formatData(r.dataInicio || r.DataInicio);
              const dataFim = formatData(r.dataFim || r.DataFim);
              const casaId = r.casaID || r.CasaID;

              const temAvaliacao = r.temAvaliacao || r.TemAvaliacao || false;
              const avaliacaoId = r.avaliacaoID || r.AvaliacaoID || null;
              const avaliacaoDaReservaAtiva = avaliacaoLoadingId === casaId;
              const emEdicaoReserva = editandoId === id;

              return (
                <div key={id} className={styles.card}>
                  <h2 className={styles.cardTitle}>{r.casaTitulo || r.CasaTitulo}</h2>
                  <p className={styles.cardAddress}>
                    {r.casaMorada || r.CasaMorada}
                  </p>

                  {emEdicaoReserva ? (
                    <div className={styles.reservaForm}>
                      <div className={styles.reservaRow}>
                        <div className={styles.reservaField}>
                          <label>Data de início</label>
                          <input
                            type="date"
                            value={editDataInicio}
                            onChange={(e) => setEditDataInicio(e.target.value)}
                          />
                        </div>
                        <div className={styles.reservaField}>
                          <label>Data de fim</label>
                          <input
                            type="date"
                            value={editDataFim}
                            onChange={(e) => setEditDataFim(e.target.value)}
                          />
                        </div>
                      </div>
                      <div
                        className={styles.buttonContainer}
                        style={{ marginTop: '0.75rem', justifyContent: 'flex-start' }}
                      >
                        <button
                          type="button"
                          className={`${styles.button} ${styles.buttonPrimary}`}
                          onClick={() => salvarEdicao(id)}
                        >
                          Guardar
                        </button>
                        <button
                          type="button"
                          className={`${styles.button} ${styles.buttonSecondary}`}
                          onClick={cancelarEdicao}
                        >
                          Cancelar
                        </button>
                      </div>
                    </div>
                  ) : (
                    <>
                      <p className={styles.cardDesc}>
                        De {dataIni} a {dataFim}
                      </p>
                      <p className={styles.cardPrice}>
                        Estado:{' '}
                        <span className={styles.reservaEstado}>
                          {estado}
                        </span>
                      </p>

                      <div
                        className={styles.buttonContainer}
                        style={{ marginTop: '0.75rem', justifyContent: 'flex-start' }}
                      >
                        {estado === 'Pendente' && (
                          <>
                            <button
                              type="button"
                              className={`${styles.button} ${styles.buttonSecondary}`}
                              onClick={() => iniciarEdicao(r)}
                            >
                              Editar
                            </button>
                            <button
                              type="button"
                              className={`${styles.button} ${styles.buttonSecondary}`}
                              onClick={() => cancelarReserva(id)}
                            >
                              Cancelar
                            </button>
                          </>
                        )}

                        {estado === 'Terminada' && (
                          <>
                            {!temAvaliacao && !avaliacaoDaReservaAtiva && (
                              <button
                                type="button"
                                className={`${styles.button} ${styles.buttonPrimary}`}
                                onClick={() => iniciarAvaliacao(r)}
                              >
                                Fazer avaliação
                              </button>
                            )}

                            {temAvaliacao && !avaliacaoDaReservaAtiva && (
                              <>
                                <button
                                  type="button"
                                  className={`${styles.button} ${styles.buttonSecondary}`}
                                  onClick={() => iniciarEdicaoAvaliacao(r)}
                                >
                                  Editar avaliação
                                </button>
                                <button
                                  type="button"
                                  className={`${styles.button} ${styles.buttonSecondary}`}
                                  onClick={() => handleApagarAvaliacao(avaliacaoId)}
                                >
                                  Apagar avaliação
                                </button>
                              </>
                            )}
                          </>
                        )}
                      </div>

                      {estado === 'Terminada' && avaliacaoDaReservaAtiva && (
                        <div className={styles.reservaForm} style={{ marginTop: '0.75rem' }}>
                          <div className={styles.reservaRow}>
                            <div className={styles.reservaField}>
                              <label>Nota (0 a 5)</label>
                              <input
                                type="number"
                                min="0"
                                max="5"
                                step="1"
                                value={avaliacaoNota}
                                onChange={(e) => setAvaliacaoNota(parseInt(e.target.value, 10) || 0)}
                              />
                            </div>
                          </div>
                          <div className={styles.reservaField} style={{ marginTop: '0.5rem' }}>
                            <label>Comentário</label>
                            <textarea
                              rows="3"
                              value={avaliacaoComentario}
                              onChange={(e) => setAvaliacaoComentario(e.target.value)}
                              style={{ width: '100%', resize: 'vertical' }}
                            />
                          </div>
                          <div
                            className={styles.buttonContainer}
                            style={{ marginTop: '0.75rem', justifyContent: 'flex-start' }}
                          >
                            {!temAvaliacao ? (
                              <button
                                type="button"
                                className={`${styles.button} ${styles.buttonPrimary}`}
                                onClick={() => handleCriarAvaliacao(casaId)}
                              >
                                Enviar avaliação
                              </button>
                            ) : (
                              <button
                                type="button"
                                className={`${styles.button} ${styles.buttonPrimary}`}
                                onClick={() => handleEditarAvaliacao(avaliacaoId)}
                              >
                                Guardar alterações
                              </button>
                            )}
                            <button
                              type="button"
                              className={`${styles.button} ${styles.buttonSecondary}`}
                              onClick={cancelarAvaliacao}
                            >
                              Fechar
                            </button>
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              );
            })}
          </div>
        )}
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

export default MinhasReservas;
