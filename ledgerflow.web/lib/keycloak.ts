import Keycloak from 'keycloak-js';

const keycloakConfig = {
    url: process.env.KEYCLOAK_URL || 'http://localhost:2000',
    realm: 'ledgerflow',
    clientId: 'ledgerflow',
};

const keycloak = new Keycloak(keycloakConfig);

export default keycloak;
