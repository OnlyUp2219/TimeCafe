import {useMemo, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";
import {authApi} from "../../shared/api/auth/authApi";
import {normalizeUnknownError} from "../../shared/api/errors/normalize";
import {httpClient} from "../../shared/api/httpClient";
import {getJwtInfo} from "../../shared/auth/jwt";
import {setAccessToken, setEmail, setEmailConfirmed, setRole, setUserId} from "../../store/authSlice";
import {Body2, Button, Caption1, Field, Input, Subtitle2Stronger, Textarea, Title3} from "@fluentui/react-components";
import {EmailInput, PasswordInput} from "../../components/FormFields";

const nowIso = () => new Date().toISOString();

export const JwtCrossServiceTestPage = () => {
    const dispatch = useDispatch();

    const currentToken = useSelector((s: RootState) => s.auth.accessToken);
    const currentUserId = useSelector((s: RootState) => s.auth.userId);

    const [email, setEmailState] = useState("");
    const [password, setPasswordState] = useState("");

    const [profileEndpoint, setProfileEndpoint] = useState("/userprofile/test/auth/public-authenticated");
    const [log, setLog] = useState("");

    const tokenInfo = useMemo(() => (currentToken ? getJwtInfo(currentToken) : null), [currentToken]);

    const append = (line: string) => {
        setLog(prev => (prev ? `${prev}\n${line}` : line));
    };

    const handleLogin = async () => {
        append(`[${nowIso()}] LOGIN: start (${email})`);
        try {
            const res = await authApi.loginJwtV2({email, password});

            const token = res.accessToken;
            dispatch(setAccessToken(token));

            const info = getJwtInfo(token);
            if (info.userId) dispatch(setUserId(info.userId));
            if (info.role) dispatch(setRole(info.role));
            if (info.email) dispatch(setEmail(info.email));
            if (typeof res.emailConfirmed === "boolean") dispatch(setEmailConfirmed(res.emailConfirmed));

            append(`[${nowIso()}] LOGIN: ok, tokenLen=${token?.length ?? 0}, userId=${info.userId ?? "?"}, role=${info.role ?? "?"}`);
        } catch (e) {
            const err = normalizeUnknownError(e);
            append(`[${nowIso()}] LOGIN: ERROR status=${err.statusCode} message=${err.message}`);
        }
    };

    const handleCallUserProfile = async () => {
        append(`[${nowIso()}] USERPROFILE: GET ${profileEndpoint}`);
        try {
            const res = await httpClient.get(profileEndpoint);
            append(`[${nowIso()}] USERPROFILE: ok ${res.status}`);
            append(JSON.stringify(res.data, null, 2));
        } catch (e) {
            const err = normalizeUnknownError(e);
            append(`[${nowIso()}] USERPROFILE: ERROR status=${err.statusCode} message=${err.message}`);
            if (err.raw) {
                try {
                    append(JSON.stringify(err.raw, null, 2));
                } catch {
                    // ignore
                }
            }
        }
    };

    const handleWhoAmI = async () => {
        const endpoint = "/userprofile/test/auth/whoami";
        append(`[${nowIso()}] USERPROFILE: GET ${endpoint}`);
        try {
            const res = await httpClient.get(endpoint);
            append(`[${nowIso()}] USERPROFILE: ok ${res.status}`);
            append(JSON.stringify(res.data, null, 2));
        } catch (e) {
            const err = normalizeUnknownError(e);
            append(`[${nowIso()}] USERPROFILE: ERROR status=${err.statusCode} message=${err.message}`);
        }
    };

    return (
        <div className="max-w-4xl mx-auto p-4 flex flex-col gap-4">
            <div className="flex flex-col gap-1">
                <Title3 block>JWT тест: Auth → UserProfile</Title3>
                <Body2 block>
                    1) Логин в Auth (получить accessToken). 2) Запрос в UserProfile с Bearer. 3) Ответ/ошибка — в логе.
                </Body2>
            </div>

            <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
                <div className="border border-white/10 rounded-lg p-3 flex flex-col gap-3">
                    <Subtitle2Stronger block>1) Login (Auth)</Subtitle2Stronger>

                    <EmailInput
                        value={email}
                        onChange={setEmailState}
                        placeholder="confirmed@example.com"
                        shouldValidate={false}
                    />

                    <PasswordInput
                        value={password}
                        onChange={setPasswordState}
                        shouldValidate={false}
                    />

                    <div className="flex flex-wrap gap-2">
                        <Button appearance="primary" onClick={handleLogin}>
                            Login → получить JWT
                        </Button>
                        <Button onClick={async () => {
                            if (!currentToken) {
                                append(`[${nowIso()}] COPY: no token`);
                                return;
                            }
                            try {
                                await navigator.clipboard.writeText(currentToken);
                                append(`[${nowIso()}] COPY: token copied (len=${currentToken.length})`);
                            } catch (err) {
                                append(`[${nowIso()}] COPY: failed to copy`);
                            }
                        }}>
                            Copy token
                        </Button>
                    </div>

                    <div className="flex flex-col gap-1">
                        <Caption1 block>Redux tokenLen: {currentToken?.length ?? 0}</Caption1>
                        <Caption1 block>Redux userId: {currentUserId || "-"}</Caption1>
                        <Caption1 block>JWT userId: {tokenInfo?.userId ?? "-"}</Caption1>
                        <Caption1 block>JWT email: {tokenInfo?.email ?? "-"}</Caption1>
                        <Caption1 block>JWT role: {tokenInfo?.role ?? "-"}</Caption1>
                        <Caption1 block>JWT exp: {tokenInfo?.exp ?? "-"}</Caption1>
                    </div>
                </div>

                <div className="border border-white/10 rounded-lg p-3 flex flex-col gap-3">
                    <Subtitle2Stronger block>2) Call UserProfile</Subtitle2Stronger>

                    <Field label="Endpoint">
                        <Input
                            value={profileEndpoint}
                            onChange={(_, data) => setProfileEndpoint(data.value)}
                            placeholder="/userprofile/test/auth/public-authenticated"
                        />
                    </Field>

                    <div className="flex flex-wrap gap-2">
                        <Button onClick={handleCallUserProfile}>GET endpoint</Button>
                        <Button onClick={handleWhoAmI}>GET whoami</Button>
                        <Button onClick={() => setLog("")}>Очистить лог</Button>
                    </div>
                </div>
            </div>

            <Field label="Лог / Ошибки">
                <Textarea
                    value={log}
                    readOnly
                    placeholder="Тут появятся ответы и ошибки"
                    className="min-h-[360px]"
                />
            </Field>
        </div>
    );
};
