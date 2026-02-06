export type User = {
    id: number;
    displayName: string;
    email: string;
    token: string;
    imageUrl?: string;
}

export type UserLogin = {
    email: string;
    password: string;
}

export type UserRegister = {
    displayName: string;
    email: string;
    password: string;
}