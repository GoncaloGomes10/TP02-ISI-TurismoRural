import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './casaadmin.module.css';

const CasasAdmin = () => {
  const [casas, setCasas] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');

  const [editingCasa, setEditingCasa] = useState(null);   
  const [showFormModal, setShowFormModal] = useState(false);

  const [deleteId, setDeleteId] = useState(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const [imagemFile, setImagemFile] = useState(null);
  const [imagemErro, setImagemErro] = useState('');

  const navigate = useNavigate();

  const carregarCasas = async () => {
    setErro('');
    setMensagem('');
    try {
      const resp = await fetch('http://localhost:5211/api/Casas/Casas');
      if (resp.ok) {
        const data = await resp.json();
        setCasas(data);
      } else {
        setErro('Erro ao carregar casas.');
      }
    } catch (e) {
      console.error(e);
      setErro('Erro de conexão.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    carregarCasas();
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    navigate('/');
  };

  const goBack = () => navigate('/casas');

  // --------- CRIAR / EDITAR ---------

  const abrirNovaCasa = () => {
    setEditingCasa({
      casaID: 0,
      titulo: '',
      descricao: '',
      tipo: 'Moradia',
      tipologia: 'T1',
      preco: 0,
      morada: '',
      codigoPostal: ''
    });
    setImagemFile(null);
    setImagemErro('');
    setErro('');
    setMensagem('');
    setShowFormModal(true);
  };

  const abrirEditarCasa = (c) => {
    setEditingCasa({
      casaID: c.casaID || c.CasaID,
      titulo: c.titulo || c.Titulo,
      descricao: c.descricao || c.Descricao,
      tipo: c.tipo || c.Tipo,
      tipologia: c.tipologia || c.Tipologia,
      preco: c.preco || c.Preco,
      morada: c.morada || c.Morada,
      codigoPostal: c.codigoPostal || c.CodigoPostal
    });
    setImagemFile(null);
    setImagemErro('');
    setErro('');
    setMensagem('');
    setShowFormModal(true);
  };

  const fecharFormModal = () => {
    setShowFormModal(false);
    setEditingCasa(null);
    setImagemFile(null);
    setImagemErro('');
  };

  const onChangeCampo = (campo, valor) => {
    setEditingCasa(prev => ({ ...prev, [campo]: valor }));
  };

  const guardarCasa = async () => {
    if (!editingCasa) return;

    setErro('');
    setMensagem('');
    setImagemErro('');

    const token = localStorage.getItem('accessToken');
    const body = {
      titulo: editingCasa.titulo,
      descricao: editingCasa.descricao,
      tipo: editingCasa.tipo,
      tipologia: editingCasa.tipologia,
      preco: Number(editingCasa.preco),
      morada: editingCasa.morada,
      codigoPostal: editingCasa.codigoPostal
    };

    const isNovo = !editingCasa.casaID;
    const url = isNovo
      ? 'http://localhost:5211/api/Casas/CriarCasa'
      : `http://localhost:5211/api/Casas/EditarCasa/${editingCasa.casaID}`;
    const method = isNovo ? 'POST' : 'PUT';

    try {
      const resp = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      });

      if (!resp.ok) {
        const text = await resp.text();
        setErro(text || 'Erro ao guardar casa.');
        return;
      }

      setMensagem(isNovo ? 'Casa criada com sucesso.' : 'Casa editada com sucesso.');

      // recarregar lista para garantir que temos os dados atualizados
      await carregarCasas();

      // obter id da casa para upload da imagem
      let casaIdParaImagem = editingCasa.casaID;

      if (isNovo) {
        // tentativa simples: encontrar a casa pela combinação título+morada e maior ID
        const encontradas = casas.filter(c =>
          (c.titulo || c.Titulo) === editingCasa.titulo &&
          (c.morada || c.Morada) === editingCasa.morada
        );

        if (encontradas.length > 0) {
          const ultima = encontradas.sort(
            (a, b) => (b.casaID || b.CasaID) - (a.casaID || a.CasaID)
          )[0];
          casaIdParaImagem = ultima.casaID || ultima.CasaID;
        }
      }

      // se tiver ficheiro e tivermos id da casa, tentar upload
      if (imagemFile && casaIdParaImagem) {
        try {
          const formData = new FormData();
          formData.append('file', imagemFile);

          // ajusta o endpoint se o teu controller estiver noutro route
          const respImg = await fetch(
            `http://localhost:5211/api/Casa_Img/upload/${casaIdParaImagem}`,
            {
              method: 'POST',
              headers: {
                'Authorization': `Bearer ${token}`
              },
              body: formData
            }
          );

          if (!respImg.ok) {
            const textImg = await respImg.text();
            setImagemErro(textImg || 'Casa guardada, mas falhou o upload da imagem.');
          } else {
            setMensagem(prev =>
              prev ? prev + ' Imagem enviada com sucesso.' : 'Imagem enviada com sucesso.'
            );
          }
        } catch (e) {
          console.error(e);
          setImagemErro('Casa guardada, mas houve erro de conexão ao enviar a imagem.');
        }
      }

      fecharFormModal();
      await carregarCasas();
    } catch (e) {
      console.error(e);
      setErro('Erro de conexão.');
    }
  };

  // --------- APAGAR ---------

  const abrirDeleteCasa = (id) => {
    setDeleteId(id);
    setErro('');
    setMensagem('');
    setShowDeleteModal(true);
  };

  const fecharDeleteModal = () => {
    setShowDeleteModal(false);
    setDeleteId(null);
  };

  const confirmarApagarCasa = async () => {
    if (!deleteId) return;

    setErro('');
    setMensagem('');

    try {
      const token = localStorage.getItem('accessToken');
      const resp = await fetch(`http://localhost:5211/api/Casas/DeleteCasa/${deleteId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      const text = await resp.text();
      if (resp.ok) {
        setMensagem(text || 'Casa apagada com sucesso.');
        fecharDeleteModal();
        await carregarCasas();
      } else {
        setErro(text || 'Erro ao apagar casa.');
      }
    } catch (e) {
      console.error(e);
      setErro('Erro de conexão.');
    }
  };

  if (isLoading) {
    return (
      <div className={styles.container}>
        <main className={styles.main}>
          <p>A carregar casas...</p>
        </main>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <div className={styles.logo} onClick={() => navigate('/')} style={{ cursor: 'pointer' }}>
            <div className={styles.logoIcon}>🏕️</div>
            <div className={styles.logoText}>Turismo Rural</div>
          </div>
          <button className={styles.logoutButton} onClick={handleLogout}>
            Sair
          </button>
        </div>
      </header>

      <main className={styles.main}>
        <h1 className={styles.title}>Gestão de casas</h1>

        {mensagem && <div className={styles.messageSuccess}>{mensagem}</div>}
        {erro && <div className={styles.error}>{erro}</div>}

        <div className={styles.buttonContainer} style={{ marginBottom: '1rem' }}>
          <button
            type="button"
            className={`${styles.button} ${styles.buttonPrimary}`}
            onClick={abrirNovaCasa}
          >
            Nova casa
          </button>
          <button
            type="button"
            className={`${styles.button} ${styles.buttonSecondary}`}
            onClick={goBack}
          >
            Voltar às casas
          </button>
        </div>

        {/* Lista de casas */}
        <div className={styles.grid}>
          {casas.map((c) => {
            const id = c.casaID || c.CasaID;
            return (
              <div key={id} className={styles.card}>
                <h2 className={styles.cardTitle}>{c.titulo || c.Titulo}</h2>
                <p className={styles.cardAddress}>{c.morada || c.Morada}</p>
                <p className={styles.cardDesc}>
                  {c.tipo || c.Tipo} {c.tipologia || c.Tipologia} - {c.preco || c.Preco} €
                </p>
                <div className={styles.buttonContainer}>
                  <button
                    type="button"
                    className={`${styles.button} ${styles.buttonSecondary}`}
                    onClick={() => abrirEditarCasa(c)}
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    className={`${styles.button} ${styles.buttonSecondary}`}
                    onClick={() => abrirDeleteCasa(id)}
                  >
                    Apagar
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </main>

      {/* Modal criar/editar casa */}
      {showFormModal && editingCasa && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h2 className={styles.modalTitle}>
              {editingCasa.casaID ? 'Editar casa' : 'Nova casa'}
            </h2>

            <div className={styles.reservaField}>
              <label>Título</label>
              <input
                type="text"
                value={editingCasa.titulo}
                onChange={(e) => onChangeCampo('titulo', e.target.value)}
              />
            </div>

            <div className={styles.reservaField}>
              <label>Descrição</label>
              <textarea
                rows="3"
                value={editingCasa.descricao}
                onChange={(e) => onChangeCampo('descricao', e.target.value)}
              />
            </div>

            <div className={styles.reservaField}>
              <label>Tipo</label>
              <select
                value={editingCasa.tipo}
                onChange={(e) => onChangeCampo('tipo', e.target.value)}
              >
                <option value="Moradia">Moradia</option>
                <option value="Apartamento">Apartamento</option>
              </select>
            </div>

            <div className={styles.reservaField}>
              <label>Tipologia</label>
              <select
                value={editingCasa.tipologia}
                onChange={(e) => onChangeCampo('tipologia', e.target.value)}
              >
                <option value="T1">T1</option>
                <option value="T2">T2</option>
                <option value="T3">T3</option>
                <option value="T4">T4</option>
              </select>
            </div>

            <div className={styles.reservaField}>
              <label>Preço por noite (€)</label>
              <input
                type="number"
                min="0"
                value={editingCasa.preco}
                onChange={(e) => onChangeCampo('preco', e.target.value)}
              />
            </div>

            <div className={styles.reservaField}>
              <label>Morada</label>
              <input
                type="text"
                value={editingCasa.morada}
                onChange={(e) => onChangeCampo('morada', e.target.value)}
              />
            </div>

            <div className={styles.reservaField}>
              <label>Código Postal</label>
              <input
                type="text"
                value={editingCasa.codigoPostal}
                onChange={(e) => onChangeCampo('codigoPostal', e.target.value)}
              />
            </div>

            <div className={styles.reservaField}>
              <label>Imagem (opcional)</label>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setImagemFile(e.target.files[0] || null)}
              />
            </div>

            {imagemErro && (
              <div className={styles.error} style={{ marginTop: '0.5rem' }}>
                {imagemErro}
              </div>
            )}

            <div className={styles.buttonContainer} style={{ marginTop: '1rem' }}>
              <button
                type="button"
                className={`${styles.button} ${styles.buttonPrimary}`}
                onClick={guardarCasa}
              >
                Guardar
              </button>
              <button
                type="button"
                className={`${styles.button} ${styles.buttonSecondary}`}
                onClick={fecharFormModal}
              >
                Cancelar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal apagar casa */}
      {showDeleteModal && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h2 className={styles.modalTitle}>Apagar casa</h2>
            <p>Tem a certeza que pretende apagar esta casa?</p>
            <div className={styles.buttonContainer} style={{ marginTop: '1rem' }}>
              <button
                type="button"
                className={`${styles.button} ${styles.buttonPrimary}`}
                onClick={confirmarApagarCasa}
              >
                Confirmar
              </button>
              <button
                type="button"
                className={`${styles.button} ${styles.buttonSecondary}`}
                onClick={fecharDeleteModal}
              >
                Cancelar
              </button>
            </div>
          </div>
        </div>
      )}

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

export default CasasAdmin;
