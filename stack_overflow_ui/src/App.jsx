import React, { useEffect, Suspense } from "react";
import {
  createBrowserRouter,
  createRoutesFromElements,
  RouterProvider,
} from "react-router-dom";
import { checkAndCleanToken } from "./services/AuthService";
import { routes } from "./route/routes";

const router = createBrowserRouter(createRoutesFromElements(<>{routes}</>));

const App = () => {
  useEffect(() => {
    const preload = async () => {
      import("./pages/HomePage");
      checkAndCleanToken();
    };
    preload();
  }, []);

  return (
    <Suspense fallback={<div>Loading...</div>}>
      <RouterProvider router={router} />
    </Suspense>
  );
};

export default App;
