export type User = {
    id: string;
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
    gender: string;
    dateOfBirth: string;
    city: string;
    country: string;
}