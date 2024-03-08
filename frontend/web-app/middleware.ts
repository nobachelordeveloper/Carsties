export { default } from "next-auth/middleware";

export const config = {
  // authentication required to go in url's in matcher
  matcher: ["/session"],
  pages: {
    signIn: "/api/auth/signin",
    // when authentication is not met, it redirects to page.tsx in "signin" folder
  },
};
