import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './perfil.module.css';

const Perfil = () => {
  const navigate = useNavigate();
  
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    telemovel: '',
    palavraPasse: ''
  });
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState('');

  // Buscar dados do perfil
  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch('http://localhost:5211/api/Utilizadores/profile', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const profileData = await response.json();
        // profileData vem em camelCase: { nome, email, telemovel }
        setUser(profileData);
        setFormData({
          nome: profileData.nome || '',
          email: profileData.email || '',
          telemovel: profileData.telemovel || '',
          palavraPasse: ''
        });
      } else {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        navigate('/login');
      }
    } catch (error) {
      console.error('Erro ao carregar perfil:', error);
      navigate('/login');
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.nome.trim()) {
      newErrors.nome = 'Nome é obrigatório';
    }
    
    if (!formData.email.trim()) {
      newErrors.email = 'Email é obrigatório';
    } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }
    
    if (!formData.telemovel.trim()) {
      newErrors.telemovel = 'Telemóvel é obrigatório';
    }
    
    if (formData.palavraPasse && formData.palavraPasse.length < 6) {
      newErrors.palavraPasse = 'Palavra-passe deve ter pelo menos 6 caracteres';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const newErrors = validateForm();
    
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch('http://localhost:5211/api/Utilizadores/update', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          nome: formData.nome,
          email: formData.email,
          telemovel: formData.telemovel,
          palavraPass: formData.palavraPasse 
        })
      });

      if (response.ok) {
        const msg = await response.text();
        setMessage(msg);
        setIsEditing(false);
        setTimeout(fetchProfile, 1500);
      } else {
        const errorText = await response.text();
        console.error('Erro ao atualizar perfil:', errorText);
        setErrors({ general: 'Erro ao atualizar perfil.' });
      }
    } catch (error) {
      console.error('Erro de conexão ao atualizar perfil:', error);
      setErrors({ general: 'Erro de conexão. Tente novamente.' });
    }
  };

  const handleLogout = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      await fetch('http://localhost:5211/api/Utilizadores/logout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
    } catch (error) {
      console.error('Erro no logout:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      navigate('/');
    }
  };

  const goBack = () => navigate(-1);

  if (isLoading) {
    return (
      <div className={styles.container}>
        <div className={styles.heroSection}>
          <div className={styles.message}>A carregar perfil...</div>
        </div>
      </div>
    );
  }

  if (!user) {
    return null;
  }

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
          <div>
            <button 
              onClick={goBack}
              className={styles.logoutButton}
              style={{ marginRight: '0.75rem' }}
            >
              Voltar
            </button>
            <button 
              onClick={handleLogout}
              className={styles.logoutButton}
            >
              Sair
            </button>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <section className={styles.heroSection}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Bem-vindo de volta, 
            <span className={styles.heroTitleAccent}>{user.nome}</span>
          </h1>
          <p className={styles.heroDescription}>
            Gerencie o seu perfil e as suas preferências de turismo rural
          </p>
        </div>
      </section>

      {/* Profile Section */}
      <section className={styles.profileSection}>
        <div className={styles.profileCard}>
          {message && !isEditing && (
            <div className={styles.message}>
              {message}
            </div>
          )}

          {isEditing ? (
            // Form de edição
            <form onSubmit={handleSubmit} className={styles.editForm}>
              {errors.general && (
                <div className={styles.message}>
                  {errors.general}
                </div>
              )}
              
              <div className={styles.formGroup}>
                <label>Nome</label>
                <input
                  type="text"
                  name="nome"
                  value={formData.nome}
                  onChange={handleInputChange}
                  className={styles.input}
                />
                {errors.nome && <span className={styles.errorText}>{errors.nome}</span>}
              </div>

              <div className={styles.formGroup}>
                <label>Email</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  className={styles.input}
                />
                {errors.email && <span className={styles.errorText}>{errors.email}</span>}
              </div>

              <div className={styles.formGroup}>
                <label>Telemóvel</label>
                <input
                  type="tel"
                  name="telemovel"
                  value={formData.telemovel}
                  onChange={handleInputChange}
                  className={styles.input}
                />
                {errors.telemovel && <span className={styles.errorText}>{errors.telemovel}</span>}
              </div>

              <div className={styles.formGroup}>
                <label>Nova Palavra-passe (opcional)</label>
                <input
                  type="password"
                  name="palavraPasse"
                  value={formData.palavraPasse}
                  onChange={handleInputChange}
                  className={styles.input}
                  placeholder="Deixe em branco para manter atual"
                />
                {errors.palavraPasse && <span className={styles.errorText}>{errors.palavraPasse}</span>}
              </div>

              <div className={styles.buttonContainer}>
                <button 
                  type="submit" 
                  className={`${styles.button} ${styles.buttonPrimary}`}
                  disabled={isLoading}
                >
                  Guardar Alterações
                </button>
                <button 
                  type="button"
                  onClick={() => setIsEditing(false)}
                  className={`${styles.button} ${styles.buttonSecondary}`}
                  disabled={isLoading}
                >
                  Cancelar
                </button>
              </div>
            </form>
          ) : (
            // Visualização do perfil
            <div className={styles.profileInfo}>
              <div className={styles.infoItem}>
                <label>Nome</label>
                <span>{user.nome}</span>
              </div>
              <div className={styles.infoItem}>
                <label>Email</label>
                <span>{user.email}</span>
              </div>
              <div className={styles.infoItem}>
                <label>Telemóvel</label>
                <span>{user.telemovel}</span>
              </div>
              
              <div className={styles.buttonContainer}>
                <button 
                  onClick={() => setIsEditing(true)}
                  className={`${styles.button} ${styles.buttonPrimary}`}
                >
                  Editar Perfil
                </button>
              </div>
            </div>
          )}
        </div>
      </section>
    </div>
  );
};

export default Perfil;
  