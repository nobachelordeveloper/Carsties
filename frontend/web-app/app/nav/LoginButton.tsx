"use client";

import { Button } from "flowbite-react";
import React from "react";
import { signIn } from "next-auth/react";

export default function LoginButton() {
  return (
    //"id-server" is the id of the provider in the authOptions in route.ts and decides sign in page
    // callbackUrl is the page to redirect to after login
    <Button outline onClick={() => signIn("id-server", { callbackUrl: "/" })}>
      <div>Login</div>
    </Button>
  );
}
