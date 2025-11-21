'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';
import keycloak from '../lib/keycloak';

interface KeycloakContextType {
    authenticated: boolean;
    loading: boolean;
    login: () => void;
    logout: () => void;
    token: string | undefined;
    getUserInfo: () => any;
}

const KeycloakContext = createContext<KeycloakContextType>({
    authenticated: false,
    loading: true,
    login: () => { },
    logout: () => { },
    token: undefined,
    getUserInfo: () => null,
});

export const useKeycloak = () => useContext(KeycloakContext);

export const KeycloakProvider = ({ children }: { children: React.ReactNode }) => {
    const [authenticated, setAuthenticated] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const initKeycloak = async () => {
            try {
                const auth = await keycloak.init({
                    onLoad: 'login-required'
                });
                setAuthenticated(auth);
                setLoading(false);
            } catch (error) {
                console.error('Failed to initialize Keycloak', error);
                setLoading(false);
            }
        };

        initKeycloak();
    }, []);

    const login = () => keycloak.login();
    const logout = () => keycloak.logout();
    const getUserInfo = () => keycloak.tokenParsed;

    return (
        <KeycloakContext.Provider
            value={{
                authenticated,
                loading,
                login,
                logout,
                token: keycloak.token,
                getUserInfo,
            }}
        >
            {children}
        </KeycloakContext.Provider>
    );
};
