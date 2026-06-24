import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers = config.headers || {};
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => Promise.reject(error));

api.interceptors.response.use(
    (response) => response,
    (error) => {
        const status = error?.response?.status;
        const requestUrl = error?.config?.url || '';

        // Trường hợp đăng nhập sai mật khẩu: không reset token/role + không ép điều hướng
        if (status === 401) {
            const isLoginRequest = requestUrl.includes('/auth/login') || requestUrl.includes('login');
            if (isLoginRequest) {
                return Promise.reject(error);
            }

            localStorage.removeItem('token');
            localStorage.removeItem('role');

            // thông báo người dùng
            // (tránh import antd ở đây; dùng alert fallback)
            try {
                // eslint-disable-next-line no-alert
                alert('Phiên đăng nhập đã hết hạn');
            } catch (_) {
                // ignore
            }

            // ép điều hướng
            try {
                window.location.href = '/login';
            } catch (_) {}
        }

        return Promise.reject(error);
    }
);



export default api;
