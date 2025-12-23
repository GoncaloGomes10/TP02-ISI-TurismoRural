import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './login.module.css';

const Login = () => {
  const navigate = useNavigate();
  
  const [formData, setFormData] = useState({
    email: '',
    palavraPasse: ''
  });
  
  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Limpar erro quando o utilizador começa a escrever
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.email.trim()) {
      newErrors.email = 'Email obrigatório';
    } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }
    
    if (!formData.palavraPasse.trim()) {
      newErrors.palavraPasse = 'Palavra-passe obrigatória';
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
      const response = await fetch('http://localhost:5211/api/Utilizadores/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Email: formData.email,
          PalavraPass: formData.palavraPasse,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Login bem-sucedido!', data);

        // corrigir nomes: tenta camelCase e PascalCase
        const accessToken = data.accessToken || data.AccessToken;
        const refreshToken = data.refreshToken || data.RefreshToken;

        console.log('TOKEN GUARDADO:', accessToken);

        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);

        // depois do login, manda para a página inicial (WelcomePage)
        navigate('/');
      

      } else {
        const errorData = await response.json().catch(() => ({}));
        setErrors({ general: 'Email ou palavra-passe inválidos.' });
        console.error('Erro no login:', errorData);
      }
    } catch (error) {
      setErrors({ general: 'Erro de conexão. Tente novamente.' });
      console.error('Erro na requisição:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const navigateToMain = () => {
    navigate('/');
  };

  const navigateToSignup = () => {
    navigate('/signup');
  };

  return (
    <div className={styles.container}>
      <div className={styles.formWrapper}>
        <div className={styles.formCard}>
          {/* Logo/Brand */}
          <div className={styles.brandSection}>
            <div className={styles.brandIcon}>
              <div>🏕️</div>
            </div>
            <h2 className={styles.brandText}>Turismo Rural</h2>
          </div>

          {/* Header */}
          <div className={styles.header}>
            <h1 className={styles.title}>Bem-vindo de volta</h1>
            <p className={styles.subtitle}>
              Entre na sua conta para continuar
            </p>
          </div>

          {/* Error Alert */}
          {errors.general && (
            <div className={styles.errorAlert}>
              {errors.general}
            </div>
          )}

          {/* Form */}
          <form className={styles.form} onSubmit={handleSubmit}>
            <div className={styles.inputGroup}>
              <label className={styles.label}>Email</label>
              <div className={styles.inputWrapper}>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  className={`${styles.input} ${errors.email ? styles.inputError : ''}`}
                  placeholder="seuemail@exemplo.com"
                  disabled={isLoading}
                />
                <div className={styles.inputIcon}>📧</div>
              </div>
              {errors.email && (
                <span className={styles.errorText}>{errors.email}</span>
              )}
            </div>

            <div className={styles.inputGroup}>
              <label className={styles.label}>Palavra-passe</label>
              <div className={styles.inputWrapper}>
                <input
                  type={showPassword ? 'text' : 'password'}
                  name="palavraPasse"
                  value={formData.palavraPasse}
                  onChange={handleInputChange}
                  className={`${styles.input} ${errors.palavraPasse ? styles.inputError : ''}`}
                  placeholder="••••••••"
                  disabled={isLoading}
                />
                <button
                  type="button"
                  onClick={togglePasswordVisibility}
                  className={styles.passwordToggle}
                  disabled={isLoading}
                >
                  {showPassword ? '🙈' : '👁️'}
                </button>
              </div>
              {errors.palavraPasse && (
                <span className={styles.errorText}>{errors.palavraPasse}</span>
              )}
            </div>

            {/* Form Options */}
            <div className={styles.formOptions}>
              <button
                type="button"
                className={styles.forgotPassword}
                disabled={isLoading}
              >
                Esqueceu a palavra-passe?
              </button>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              className={`${styles.submitButton} ${isLoading ? styles.submitButtonLoading : ''}`}
              disabled={isLoading}
            >
              {isLoading ? (
                <>
                  A entrar...
                </>
              ) : (
                'Entrar'
              )}
            </button>
          </form>

          {/* Footer */}
          <div className={styles.footer}>
            <div className={styles.footerLinks}>
              <button
                type="button"
                onClick={navigateToMain}
                className={styles.linkButton}
                disabled={isLoading}
              >
                Voltar à página inicial
              </button>
              <span className={styles.separator}>ou</span>
              <button
                type="button"
                onClick={navigateToSignup}
                className={styles.linkButton}
                disabled={isLoading}
              >
                Não tem conta? Criar conta
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
