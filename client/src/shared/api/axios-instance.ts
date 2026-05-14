import axios from "axios";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

export const apiClient = axios.create({
    baseURL: "http://localhost:9001/api",
    headers: { "Content-Type": "application/json" },
    timeout: 5000,
});

apiClient.interceptors.response.use(
    (response) => {
        const data = response.data as Envelope;

        if (data.isError && data.errorList) {
            throw new EnvelopeError(data.errorList);
        }

        return response;
    },
    (error) => {
        if (axios.isAxiosError(error)) {
            if (error.code === "ECONNABORTED") {
                throw new Error(
                    "Сервер не ответил за 5 секунд. Попробуйте позже.",
                );
            }
            if (!error.response) {
                throw new Error(
                    "Нет соединения с сервером. Проверьте, что бэкенд запущен.",
                );
            }
            if (error.response?.data) {
                const envelope = error.response.data as Envelope;
                if (envelope?.isError && envelope.errorList) {
                    throw new EnvelopeError(envelope.errorList);
                }
            }
        }

        return Promise.reject(error);
    },
);
