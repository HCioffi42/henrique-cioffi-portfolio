import axios from 'axios';

// Cria uma instância centralizada do Axios apontando para a sua API C# no Kestrel.
const api = axios.create({
    baseURL: 'http://localhost:25683/api'
});

export default api;