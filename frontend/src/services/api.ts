import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Response interceptor for easy data access or error handling
api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const message = error.response?.data || error.message;
    console.error('API Error:', message);
    return Promise.reject(message);
  }
);

export default api;
