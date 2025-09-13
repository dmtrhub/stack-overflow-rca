import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { isAuthenticated, getUserIdFromToken, removeToken } from '../services/AuthService';

const Navbar = () => {
  const navigate = useNavigate();
  const loggedIn = isAuthenticated();
  const userId = getUserIdFromToken();

  const handleLogout = () => {
    removeToken();
    navigate('/login');
  };

  return (
    <nav className="bg-gray-800 text-white px-4 py-3 shadow-md">
      <div className="container mx-auto flex justify-between items-center">
        <div className="text-xl font-bold">
          <Link to="/" className="text-cyan-400 hover:text-cyan-300">Stack OverFlow</Link>
        </div>

        <ul className="flex space-x-6 items-center">
          {loggedIn ? (
            <>
              <li>
                <Link to="/" className="hover:text-cyan-300">Home</Link>
              </li>
              <li>
                <Link to="/ask" className="hover:text-cyan-300">Ask Question</Link>
              </li>
              <li>
                <Link to="/my-questions" className="hover:text-cyan-300">My Questions</Link>
              </li>
              <li>
                <Link to={`/profile/${userId}`} className="hover:text-cyan-300">Profile</Link>
              </li>
              <li>
                <button
                  onClick={handleLogout}
                  className="hover:text-red-400 transition-colors"
                >
                  LogOut
                </button>
              </li>
            </>
          ) : (
            <>
              <li>
                <Link to="/login" className="hover:text-cyan-300">Login</Link>
              </li>
              <li>
                <Link to="/register" className="hover:text-cyan-300">Register</Link>
              </li>
            </>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
