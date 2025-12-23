import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import styles from './casas.module.css';

const CasaDetalhes = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [casa, setCasa] = useState(null);
  const [imagemUrl, setImagemUrl] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const [dataInicio, setDataInicio] = useState('');
  const [dataFim, setDataFim] = useState('');
  const [reservaMensagem, setReservaMensagem] = useState('');
  const [reservaErro, setReservaErro] = useState('');
  const [isReservando, setIsReservando] = useState(false);
  const [reservasCasa, setReservasCasa] = useState([]);

  const [avaliacoes, setAvaliacoes] = useState([]);
  const [isLoadingAvaliacoes, setIsLoadingAvaliacoes] = useState(true);

  useEffect(() => {
    const fetchCasa = async () => {
      try {
        const response = await fetch(`http://localhost:5211/api/Casas/${id}`);
        if (response.ok) {
          const data = await response.json();
          setCasa(data);
        } else {
          setError('Casa não encontrada.');
        }
      } catch (err) {
        console.error('Erro ao carregar casa:', err);
        setError('Erro de conexão. Tente novamente.');
      } finally {
        setIsLoading(false);
      }
    };

    const fetchImagem = async () => {
      try {
        const resp = await fetch(`http://localhost:5211/api/Casa_Img/${id}`);
        if (resp.ok) {
          const imgs = await resp.json();
          if (imgs && imgs.length > 0) {
            const img = imgs[0];
            setImagemUrl(
              img.url ||
              img.Url ||
              img.pathImagem ||
              img.PathImagem
            );
          }
        }
      } catch (err) {
        console.error('Erro ao carregar imagem da casa:', err);
      }
    };

    const atualizarEstadosReservas = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        if (!token) return;

        await fetch('http://localhost:5211/api/Reservas/AtualizarEstados', {
          method: 'PUT',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
      } catch (err) {
        console.error('Erro ao atualizar estados das reservas:', err);
      }
    };

    const fetchReservasCasa = async () => {
      try {
        const resp = await fetch(`http://localhost:5211/api/Reservas/PorCasa/${id}`);
        if (resp.ok) {
          const data = await resp.json();
          setReservasCasa(data);
        }
      } catch (err) {
        console.error('Erro ao carregar reservas da casa:', err);
      }
    };

    const fetchAvaliacoes = async () => {
      try {
        setIsLoadingAvaliacoes(true);
        const response = await fetch(`http://localhost:5211/api/Avaliacoes/PorCasa/${id}`);
        if (response.ok) {
          const data = await response.json();
          setAvaliacoes(data);
        }
      } catch (err) {
        console.error('Erro ao carregar avaliações:', err);
      } finally {
        setIsLoadingAvaliacoes(false);
      }
    };

    (async () => {
      await atualizarEstadosReservas();
      fetchCasa();
      fetchImagem();
      fetchReservasCasa();
      fetchAvaliacoes();
    })();
  }, [id]);

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
      navigate('/');
    }
  };

  const goHome = () => navigate('/');
  const goCasas = () => navigate('/casas');

  const parseDate = (str) => {
    if (!str) return null;
    return new Date(str.toString().split('T')[0] + 'T00:00:00');
  };

  const isDateBlocked = (dateStr) => {
    if (!dateStr) return false;
    const d = parseDate(dateStr);
    if (!d) return false;

    return reservasCasa.some((r) => {
      const start = parseDate(r.dataInicio || r.DataInicio);
      const end = parseDate(r.dataFim || r.DataFim);
      if (!start || !end) return false;
      return d >= start && d < end;
    });
  };

  const handleCriarReserva = async (e) => {
    e.preventDefault();
    setReservaMensagem('');
    setReservaErro('');

    if (!dataInicio || !dataFim) {
      setReservaErro('Por favor selecione as datas de início e fim.');
      return;
    }

    if (isDateBlocked(dataInicio) || isDateBlocked(dataFim)) {
      setReservaErro('As datas selecionadas já estão reservadas para esta casa.');
      return;
    }

    setIsReservando(true);
    try {
      const body = {
        casaID: parseInt(id, 10),
        dataInicio: dataInicio,
        dataFim: dataFim
      };

      const token = localStorage.getItem('accessToken');

      const response = await fetch('http://localhost:5211/api/Reservas/CriarReserva', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      });

      if (response.ok) {
        const text = await response.text();
        setReservaMensagem(text || 'Reserva criada com sucesso!');
        const resp = await fetch(`http://localhost:5211/api/Reservas/PorCasa/${id}`);
        if (resp.ok) {
          const data = await resp.json();
          setReservasCasa(data);
        }
      } else {
        const text = await response.text();
        console.error('Erro ao criar reserva:', text);
        setReservaErro(text || 'Erro ao criar reserva.');
      }
    } catch (err) {
      console.error('Erro na reserva:', err);
      setReservaErro('Erro de conexão. Tente novamente.');
    } finally {
      setIsReservando(false);
    }
  };

  const calcularMediaAvaliacoes = () => {
    if (avaliacoes.length === 0) return 0;
    const soma = avaliacoes.reduce((acc, a) => acc + Number(a.nota), 0);
    return (soma / avaliacoes.length).toFixed(1);
  };

  const renderStars = (nota) => {
    const valor = Number(nota) || 0;
    return Array.from({ length: 5 }, (_, i) => (
      <span
        key={i}
        className={valor > i ? styles.starFilled : styles.starEmpty}
        title={`${i + 1} estrelas`}
      >
        ★
      </span>
    ));
  };

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
          </div>
        </header>
        <main className={styles.main}>
          <p>A carregar casa...</p>
        </main>
      </div>
    );
  }

  if (!casa) {
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
          <p>{error || 'Casa não encontrada.'}</p>
        </main>
      </div>
    );
  }

  const titulo = casa.titulo || casa.Titulo;
  const tipo = casa.tipo || casa.Tipo;
  const tipologia = casa.tipologia || casa.Tipologia;
  const descricao = casa.descricao || casa.Descricao;
  const preco = casa.preco || casa.Preco;
  const morada = casa.morada || casa.Morada;
  const codigoPostal = casa.codigoPostal || casa.CodigoPostal;

  const formatData = (d) => {
    if (!d) return '';
    return (d.toString().split('T')[0]);
  };

  const hojeStr = new Date().toISOString().split('T')[0];
  const datasConflito =
    dataInicio && dataFim && (isDateBlocked(dataInicio) || isDateBlocked(dataFim));

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
            <button
              className={styles.backButton}
              onClick={goCasas}
              style={{ marginRight: '0.75rem' }}
            >
              Voltar às casas
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

      {/* Conteúdo da Casa */}
      <main className={styles.main}>
        <h1 className={styles.title}>{titulo}</h1>
        <p className={styles.subtitle}>
          {tipo} · {tipologia}
        </p>

        {imagemUrl && (
          <img
            src={imagemUrl}
            alt={titulo}
            className={styles.detalheImagem}
          />
        )}

        <div className={styles.card}>
          <p className={styles.cardDesc}>{descricao}</p>
          <p className={styles.cardPrice}>{preco} € / noite</p>
          <p className={styles.cardAddress}>
            {morada}, {codigoPostal}
          </p>
        </div>

        {/* Avaliações */}
        <div className={styles.avaliacoesCard}>
          <div className={styles.avaliacoesHeader}>
            <h2 className={styles.avaliacoesTitle}>Avaliações</h2>
            {avaliacoes.length > 0 && (
              <div className={styles.mediaContainer}>
                <div className={styles.mediaStars}>
                  {renderStars(Number(calcularMediaAvaliacoes()))}
                </div>
                <span className={styles.mediaTexto}>
                  {calcularMediaAvaliacoes()} ({avaliacoes.length})
                </span>
              </div>
            )}
          </div>

          {isLoadingAvaliacoes ? (
            <p className={styles.loadingText}>A carregar avaliações...</p>
          ) : avaliacoes.length === 0 ? (
            <p className={styles.semAvaliacoes}>Nenhuma avaliação.</p>
          ) : (
            <div className={styles.listaAvaliacoes}>
              {avaliacoes.map((avaliacao) => (
                <div key={avaliacao.avaliacaoID} className={styles.avaliacaoItem}>
                  <div className={styles.avaliacaoHeader}>
                    <span className={styles.nomeUtilizador}>
                      {avaliacao.nomeUtilizador}
                    </span>
                    <div className={styles.starsContainer}>
                      {renderStars(Number(avaliacao.nota))}
                      <span className={styles.notaNum}>
                        {Number(avaliacao.nota)}
                      </span>
                    </div>
                  </div>
                  <p className={styles.comentario}>{avaliacao.comentario}</p>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Reserva */}
        <div className={styles.reservaCard}>
          <h2 className={styles.reservaTitle}>Fazer reserva</h2>

          {reservaMensagem && (
            <div className={styles.messageSuccess}>
              {reservaMensagem}
            </div>
          )}
          {reservaErro && (
            <div className={styles.error}>
              {reservaErro}
            </div>
          )}

          <form onSubmit={handleCriarReserva} className={styles.reservaForm}>
            <div className={styles.reservaRow}>
              <div className={styles.reservaField}>
                <label>Data de início</label>
                <input
                  type="date"
                  value={dataInicio}
                  onChange={(e) => setDataInicio(e.target.value)}
                  required
                  min={hojeStr}
                />
              </div>
              <div className={styles.reservaField}>
                <label>Data de fim</label>
                <input
                  type="date"
                  value={dataFim}
                  onChange={(e) => setDataFim(e.target.value)}
                  required
                  min={dataInicio || hojeStr}
                />
              </div>
            </div>

            <button
              type="submit"
              className={styles.buttonPrimary}
              disabled={isReservando}
              style={{ marginTop: '1rem', minWidth: '220px' }}
            >
              {isReservando ? 'A reservar...' : 'Reservar esta casa'}
            </button>
          </form>

          {datasConflito && (
            <p className={styles.reservaWarning}>
              As datas escolhidas incluem dias que já estão reservados para esta casa.
            </p>
          )}

          <div className={styles.reservasLista}>
            <h3>Reservas existentes</h3>
            {reservasCasa.length === 0 ? (
              <p className={styles.reservasEmpty}>
                Ainda não existem reservas para esta casa.
              </p>
            ) : (
              <ul>
                {reservasCasa.map((r) => (
                  <li key={r.reservaID || r.ReservaID}>
                    <span>
                      {formatData(r.dataInicio || r.DataInicio)} até{' '}
                      {formatData(r.dataFim || r.DataFim)}
                    </span>
                    <span className={styles.reservaEstado}>
                      {r.estado || r.Estado}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
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

export default CasaDetalhes;
