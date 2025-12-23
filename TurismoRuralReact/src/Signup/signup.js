import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './signup.module.css';

const Signup = () => {
  const navigate = useNavigate();
  
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    telemovel: '',
    palavraPasse: ''
  });

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Limpar erro quando o utilizador começa a escrever
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.nome.trim()) {
      newErrors.nome = 'Nome é obrigatório';
    }

    if (!formData.email.trim()) {
      newErrors.email = 'Email é obrigatório';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }

    if (!formData.telemovel.trim()) {
      newErrors.telemovel = 'Telemóvel é obrigatório';
    }

    if (!formData.palavraPasse.trim()) {
      newErrors.palavraPasse = 'Palavra-passe é obrigatória';
    } else if (formData.palavraPasse.length < 6) {
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

    setIsLoading(true);
    
    try {
      const response = await fetch('http://localhost:5211/api/Utilizadores/signup', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Nome: formData.nome,
          Email: formData.email,
          Telemovel: formData.telemovel,
          PalavraPass: formData.palavraPasse
        }),
      });

      if (response.ok) {
        console.log('Conta criada com sucesso!');
        navigate('/login', { 
          state: { 
            message: 'Conta criada com sucesso! Faça login para continuar.',
            email: formData.email 
          }
        });
      } else {
        const errorText = await response.text();
        let errorMsg = 'Erro ao criar conta. Tente novamente.';
        
        try {
          const errorData = JSON.parse(errorText);
          if (errorData === "Email já está registado.") {
            errorMsg = 'Este email já está registado.';
          } else if (errorData.includes('email')) {
            errorMsg = 'Formato de email inválido.';
          }
        } catch {}
        
        setErrors({ general: errorMsg });
        console.error('Erro ao criar conta:', errorText);
      }
    } catch (error) {
      setErrors({ general: 'Erro de conexão. Tente novamente.' });
      console.error('Erro na requisição:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const navigateToMain = () => {
    navigate('/');
  };

  const navigateToLogin = () => {
    navigate('/login');
  };

  return (
    <div className={styles.container}>
      <div className={styles.formWrapper}>
        <div className={styles.formCard}>
          <div className={styles.header}>
            <h1 className={styles.title}>Criar Conta</h1>
            <p className={styles.subtitle}>Junte-se ao nosso turismo rural</p>
          </div>
          
          {errors.general && (
            <div className={styles.errorAlert}>
              {errors.general}
            </div>
          )}

          <div className={styles.form}>
            <div className={styles.inputGroup}>
              <label className={styles.label}>Nome</label>
              <input
                type="text"
                name="nome"
                value={formData.nome}
                onChange={handleInputChange}
                className={`${styles.input} ${errors.nome ? styles.inputError : ''}`}
                placeholder="O seu nome"
              />
              {errors.nome && <span className={styles.errorText}>{errors.nome}</span>}
            </div>
            
            <div className={styles.inputGroup}>
              <label className={styles.label}>Email</label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleInputChange}
                className={`${styles.input} ${errors.email ? styles.inputError : ''}`}
                placeholder="seu@email.com"
              />
              {errors.email && <span className={styles.errorText}>{errors.email}</span>}
            </div>
            
            <div className={styles.inputGroup}>
              <label className={styles.label}>Telemóvel</label>
              <input
                type="tel"
                name="telemovel"
                value={formData.telemovel}
                onChange={handleInputChange}
                className={`${styles.input} ${errors.telemovel ? styles.inputError : ''}`}
                placeholder="+351 9XX XXX XXX"
              />
              {errors.telemovel && <span className={styles.errorText}>{errors.telemovel}</span>}
            </div>
            
            <div className={styles.inputGroup}>
              <label className={styles.label}>Palavra-passe</label>
              <input
                type="password"
                name="palavraPasse"
                value={formData.palavraPasse}
                onChange={handleInputChange}
                className={`${styles.input} ${errors.palavraPasse ? styles.inputError : ''}`}
                placeholder="••••••••"
              />
              {errors.palavraPasse && <span className={styles.errorText}>{errors.palavraPasse}</span>}
            </div>
            
            <button
              onClick={handleSubmit}
              disabled={isLoading}
              className={`${styles.submitButton} ${isLoading ? styles.submitButtonLoading : ''}`}
            >
              {isLoading ? 'A criar conta...' : 'Criar Conta'}
            </button>
          </div>
          
          <div className={styles.footer}>
            <div className={styles.footerLinks}>
              <button
                onClick={navigateToMain}
                className={styles.linkButton}
              >
                ← Voltar à página inicial
              </button>
              <span className={styles.separator}>|</span>
              <button
                onClick={navigateToLogin}
                className={styles.linkButton}
              >
                Já tem conta? Entrar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Signup;