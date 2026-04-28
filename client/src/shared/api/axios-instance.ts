import axios from "axios";

export const apiClient = axios.create({
    baseURL: "http://localhost:9001/api",
    headers: { "Content-Type": "application/json" },
});
