import React from 'react';
import {useOidcAccessToken, useOidcIdToken, useOidcUser} from '@axa-fr/react-oidc';
import { EnvironmentConsumer } from './EnvironmentProvider';

const NON_CONNECTE = 'Non Connecté';

const getAuthName = (dataArray, propertyName= "name", defaultResult=NON_CONNECTE) => {

    for(let i=0; i<dataArray.length;i++) {
        const item= dataArray[i];
        if (item && item[propertyName]) {
            return item[propertyName];
        }
    }

    return defaultResult;
}

export const DataScientist = "ECOTAG_DATA_SCIENTIST";
export const Annotateur = "ECOTAG_ANNOTATEUR";
export const Administateur = "ECOTAG_ADMINISTRATEUR";
    
export const extractRoles = (oidcUser, oidcMode) => {
    const roles = [];
    if(oidcMode === "AXA_FRANCE"){
          if(oidcUser && oidcUser.member_of && oidcUser.member_of.length > 0) {
              oidcUser.member_of.forEach(member => {
                  member.split(",").forEach(subMember => {
                      if(subMember.startsWith('CN=')){
                          const role = subMember.replace('CN=', '');
                          if(role.includes("ECOTAG_")) {
                              roles.push(role);
                          }
                      }
                  });
              });
              if(roles.includes(DataScientist)){
                  roles.push(Annotateur);
              }
              else if(roles.includes(Administateur)){
                  roles.push(DataScientist);
                  roles.push(Annotateur);
              }
          }
    }else{
        roles.push(Annotateur);
        roles.push(DataScientist);
        roles.push(Administateur);
    }
  return roles;
}

const extractDataFromOAuthToken = (idTokenPayload, accessTokenPayload, oidcUser, environment) => ({
  name: getAuthName([idTokenPayload, accessTokenPayload]),
  email: getAuthName([oidcUser, accessTokenPayload], 'email', ''),
  roles: extractRoles(oidcUser, environment.oidc.mode)
});

const withAuthentication = () => Component => props => {
  const { idTokenPayload } = useOidcIdToken();
  const { accessTokenPayload } = useOidcAccessToken();
  const{ oidcUser, oidcUserLoadingState } = useOidcUser();
  return <EnvironmentConsumer>{store =><Component {...props} user={extractDataFromOAuthToken(idTokenPayload, accessTokenPayload, oidcUser, store.environment)} UserLoadingState={oidcUserLoadingState} />}</EnvironmentConsumer>;
};

export default withAuthentication;
