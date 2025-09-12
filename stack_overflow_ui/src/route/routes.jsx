import React, { Children } from "react";
import { Route, Navigate } from "react-router-dom";
import { isAuthenticated } from "../services/AuthService";
import DefaultLayout from "../components/DefaultLayout";
const HomePage = React.lazy(() => import("../pages/HomePage"));
const ModifyProfilePage = React.lazy(() => import("../pages/ModifyProfilePage"));
const AnswerPage = React.lazy(() => import("../pages/AnswerPage"));
const ProfilePage = React.lazy(() => import("../pages/ProfilePage"));
const ModifyQuestionPage = React.lazy(() => import("../pages/ModifyQuestionPage"));
const RegisterPage = React.lazy(() => import("../pages/RegisterPage"));
const LoginPage = React.lazy(() => import("../pages/LoginPage"));

const PrivateRoute = ({ children }) => {
    if (!isAuthenticated()) {
        return <Navigate to="/login" replace />;
    }

    return children;
};

const GuestRoute = ({ children }) => {
    if (isAuthenticated()) {
        return <Navigate to="/" replace />;
    }

    return children;
};

export const routes = (
    <Route path="/" element={<DefaultLayout />}>
        <Route index element={<HomePage />} />
        <Route
            path="modifyProfile/:id"
            element={
                <PrivateRoute>
                    <ModifyProfilePage />
                </PrivateRoute>
            }
        />
        <Route
            path="profile/:id"
            element={
                <PrivateRoute>
                    <ProfilePage />
                </PrivateRoute>
            }
        />
        <Route
            path="question/:id/answers"
            element={
                <PrivateRoute>
                    <AnswerPage />
                </PrivateRoute>
            }
        />
        <Route
            path="question/:id/modify"
            element={
                <PrivateRoute>
                    <ModifyQuestionPage />
                </PrivateRoute>
            }
        />
        <Route
            path="register"
            element={
                <GuestRoute>
                    <RegisterPage />
                </GuestRoute>
            }
        />
        <Route
            path="login"
            element={
                <GuestRoute>
                    <LoginPage />
                </GuestRoute>
            }

        />
    </Route>
);
