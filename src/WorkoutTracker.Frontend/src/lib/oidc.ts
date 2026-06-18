// AWS Cognito OIDC settings. These are public identifiers (no client secret —
// this is a public SPA client), so they are safe to ship in the bundle.
export const cognitoConfig = {
  authority: "https://cognito-idp.eu-central-1.amazonaws.com/eu-central-1_0TLyopLpd",
  clientId: "4qbcflh9k58uceva9j6dk11vbg",
  // Hosted UI / managed login domain — used for the (non-standard) Cognito logout URL.
  domain: "https://eu-central-10tlyoplpd.auth.eu-central-1.amazoncognito.com",
  scope: "openid email profile",
};
