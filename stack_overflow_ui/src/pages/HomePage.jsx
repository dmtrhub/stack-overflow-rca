import React from "react";
import { isAuthenticated } from "../services/AuthService";
import Questions from "../components/Questions"

const HomePage = () => {
    const UserHomePage = () => {
        <>
            <div className="max-w-xl mx-auto text-center px-4 py-10">
                <h1>
                    Welcome back!
                </h1>
            </div>
            <Questions />
        </>
    }

    const GuestHomePage = () => {
        <div className="max-w-xl mx-auto text-center px-4 py-20">
            <h1 className="text-5xl font-extrabold text-gray-900 mb-6 leading-tight">
                Welcome to StackOverflow!
            </h1>
            <p className="text-lg text-gray-700 tracking-wide">
                Create an account or login to start asking questions and answering.
            </p>
        </div>
    }


    return (
        <div className="pt-50 px-5 ">
            {isAuthenticated ?
                <><UserHomePage /></> : <><GuestHomePage /></>}
        </div>
    )
}

export default HomePage;