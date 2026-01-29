import {Header} from "../../components/Header/Header";
import {Sidebar} from "../../components/Sidebar/Sidebar";
import {Footer} from "../../components/Footer/Footer";
import {Outlet} from "react-router-dom";
import {useDispatch, useSelector} from "react-redux";
import {toggleSidebar} from "../../store/uiSlice.ts";
import type {RootState} from "../../store";
import type {FC} from "react";
import {useEffect, useMemo, useState} from "react";
import {
    Button,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Input,
    Spinner,
    Text,
} from "@fluentui/react-components";
import {fetchProfileByUserId, updateProfile} from "../../store/profileSlice";
import {isNameCompleted} from "../../utility/profileCompletion";

export const MainLayout: FC = () => {
    const dispatch = useDispatch();
    const isSidebarOpen = useSelector((state: RootState) => state.ui.isSideBarOpen);

    const accessToken = useSelector((state: RootState) => state.auth.accessToken);
    const userId = useSelector((state: RootState) => state.auth.userId);
    const profile = useSelector((state: RootState) => state.profile.data);
    const profileLoading = useSelector((state: RootState) => state.profile.loading);
    const profileSaving = useSelector((state: RootState) => state.profile.saving);

    useEffect(() => {
        if (!accessToken) return;
        if (!userId) return;
        if (profile || profileLoading) return;
        void dispatch(fetchProfileByUserId({userId}) as never);
    }, [accessToken, dispatch, profile, profileLoading, userId]);

    const mustCompleteProfile = useMemo(() => {
        if (!accessToken) return false;
        if (!userId) return false;
        if (profileLoading) return true;
        if (!profile) return true;
        return !isNameCompleted(profile);
    }, [accessToken, profile, profileLoading, userId]);

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [middleName, setMiddleName] = useState('');

    useEffect(() => {
        if (!profile) return;
        setFirstName(profile.firstName ?? '');
        setLastName(profile.lastName ?? '');
        setMiddleName(profile.middleName ?? '');
    }, [profile?.firstName, profile?.lastName, profile?.middleName, profile]);

    const canSave = Boolean(firstName.trim()) && Boolean(lastName.trim()) && !profileSaving && !profileLoading;

    return (
        <div className="main-layout">
            <Header
                onMenuToggle={() => dispatch(toggleSidebar())}
                isSidebarOpen={isSidebarOpen}
            />

            <Dialog open={mustCompleteProfile} modalType="alert">
                <DialogSurface className="w-[calc(100vw-32px)] max-w-[640px]">
                    <DialogBody>
                        <DialogTitle>Заполните профиль</DialogTitle>

                        <DialogContent>
                            {profileLoading || !profile ? (
                                <div className="flex items-center gap-3">
                                    <Spinner size="small" />
                                    <Text>Загружаем профиль…</Text>
                                </div>
                            ) : (
                                <div className="flex flex-col gap-3">
                                    <Text>
                                        Это обязательный шаг. Укажите как минимум имя и фамилию.
                                    </Text>

                                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                        <Field label="Фамилия" required>
                                            <Input value={lastName} onChange={(_, d) => setLastName(d.value)} />
                                        </Field>
                                        <Field label="Имя" required>
                                            <Input value={firstName} onChange={(_, d) => setFirstName(d.value)} />
                                        </Field>
                                        <Field label="Отчество">
                                            <Input value={middleName} onChange={(_, d) => setMiddleName(d.value)} />
                                        </Field>
                                    </div>
                                </div>
                            )}
                        </DialogContent>

                        <DialogActions>
                            <Button
                                appearance="primary"
                                disabled={!canSave}
                                onClick={async () => {
                                    const action = await dispatch(
                                        updateProfile({
                                            firstName: firstName.trim(),
                                            lastName: lastName.trim(),
                                            middleName: middleName.trim() || undefined,
                                        }) as never
                                    );
                                    if (updateProfile.fulfilled.match(action)) {
                                        void dispatch(fetchProfileByUserId({userId: String(userId)}) as never);
                                    }
                                }}
                            >
                                Сохранить
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>

            <div className="main-layout__content">
                <Sidebar/>

                <main className="main-layout__main"><Outlet/></main>
            </div>

            <Footer/>
        </div>
    );
};
