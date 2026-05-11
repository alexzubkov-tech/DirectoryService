export type ApiError = {
    messages: ErrorMessage[];
    statusCode: ErrorType;
};

export type ErrorMessage = {
    code: string;
    message: string;
    invalidField?: string | null;
};

export type ErrorType =
    | "validation"
    | "not_found"
    | "failure"
    | "conflict"
    | "authentication"
    | "authorization";

export class EnvelopeError extends Error {
    public readonly errorList: ApiError[];
    public readonly type: ErrorType;

    constructor(errorList: ApiError[]) {
        const firstError = errorList[0];
        const firstMessage =
            firstError?.messages?.[0]?.message ?? "Неизвестная ошибка";

        super(firstMessage);
        this.name = "EnvelopeError";
        this.errorList = errorList;
        this.type = firstError?.statusCode ?? "failure";

        Object.setPrototypeOf(this, EnvelopeError.prototype);
    }

    get firstError(): ApiError | undefined {
        return this.errorList[0];
    }

    get firstMessage(): string {
        return this.firstError?.messages?.[0]?.message ?? "Неизвестная ошибка";
    }

    get allMessages(): string[] {
        return this.errorList.flatMap((error) =>
            error.messages.map((msg) => msg.message),
        );
    }

    getMessageByCode(code: string): string | undefined {
        for (const error of this.errorList) {
            const found = error.messages.find((msg) => msg.code === code);
            if (found) return found.message;
        }
        return undefined;
    }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
    return error instanceof EnvelopeError;
}
