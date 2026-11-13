
const ALLOWED_RETURN = /^\/(?:games|favorites|account)(?:[/?#]|$)/;

export function safeReturnUrl(
		raw: string | null,
		fallback = '/games'
	): string {
		return raw && ALLOWED_RETURN.test(raw) ? raw : fallback;
}
