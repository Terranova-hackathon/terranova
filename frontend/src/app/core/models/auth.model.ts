export interface User {
	id: string;
	email: string;
	displayName: string | null;
}

export interface LoginRequest {
	email: string;
	password: string;
}

export interface RegisterRequest extends LoginRequest {
	displayName: string | null;
}
