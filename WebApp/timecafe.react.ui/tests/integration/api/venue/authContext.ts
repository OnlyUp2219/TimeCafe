import {
    createPassword,
    createTestClient,
    createTestEmail,
    loginAndGetAccessToken,
    parseCallbackParams,
    registerUser,
} from "@tests/integration/api/helpers";

export const createAuthorizedContext = async () => {
    const {client} = createTestClient();
    const email = createTestEmail();
    const password = createPassword();

    const registerRes = await registerUser(client, email, password);
    const callbackUrl = registerRes.data?.callbackUrl as string | undefined;

    if (callbackUrl) {
        const parsed = parseCallbackParams(callbackUrl);
        if (parsed.userId && parsed.token) {
            await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
        }
    }

    const {token} = await loginAndGetAccessToken(client, email, password);

    return {
        client,
        token,
        headers: token ? {Authorization: `Bearer ${token}`} : {},
    };
};