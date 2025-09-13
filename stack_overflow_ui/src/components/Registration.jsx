import React, { useState } from "react";
import { registerUser } from "../services/UserService";
import { Link } from "react-router-dom";

const Register = () => {
  const initialFormData = {
    fullName: "",
    gender: "",
    country: "",
    city: "",
    address: "",
    email: "",
    password: "",
    profileImage: null,
  };

  const initialErrors = {
    fullName: "",
    gender: "",
    country: "",
    city: "",
    address: "",
    email: "",
    password: "",
    profileImage: "",
  };

  const [formData, setFormData] = useState(initialFormData);
  const [errors, setErrors] = useState(initialErrors);
  const [message, setMessage] = useState({ type: "", text: "" });
  const [imagePreview, setImagePreview] = useState(null);

  const validateFields = () => {
    const newErrors = { ...initialErrors };
    let isValid = true;

    for (const key in formData) {
      if (!formData[key]) {
        newErrors[key] = "This field is required.";
        isValid = false;
      }
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleChange = (e) => {
    const { name, value, files } = e.target;

    if (files) {
      const file = files[0];
      setFormData((prev) => ({
        ...prev,
        [name]: file,
      }));
      setImagePreview(URL.createObjectURL(file));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value,
      }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage({ type: "", text: "" });

    if (!validateFields()) return;

    try {
      const response = await registerUser(formData);

      if (!response.ok) {
        const result = await response.json();
        if (result.Errors) {
          const formattedErrors = { ...initialErrors };
          result.Errors.forEach((err) => {
            if (formattedErrors.hasOwnProperty(err.Field)) {
              formattedErrors[err.Field] = err.Message;
            }
          });
          setErrors(formattedErrors);
        } else {
          setMessage({
            type: "error",
            text: result.message || "Registration failed.",
          });
        }
      } else {
        setMessage({ type: "success", text: "Registration successful!" });
        setFormData(initialFormData);
        setErrors(initialErrors);
        setImagePreview(null);
      }
    } catch (error) {
      setMessage({
        type: "error",
        text: "An error occurred. Please try again.",
      });
    }
  };

  return (
    <div className="max-w-xl mx-auto p-8 bg-white shadow-lg rounded-xl mt-10">
      <Link
        to="/"
        className="block w-full text-2xl font-semibold mb-6 text-center"
      >
        StackOverflow
      </Link>
      <form
        onSubmit={handleSubmit}
        encType="multipart/form-data"
        className="space-y-5"
      >
        {/* Profile Image Upload */}
        {
          <div>
            <label className="block font-semibold mb-1">Profile Image</label>

            <input
              id="profileImage"
              type="file"
              name="profileImage"
              accept="image/*"
              onChange={handleChange}
              className="hidden"
            />

            <label
              htmlFor="profileImage"
              className="inline-block bg-blue-600 text-white px-4 py-2 rounded cursor-pointer hover:bg-blue-700 transition"
            >
              Choose Image
            </label>

            {imagePreview && (
              <img
                src={imagePreview}
                alt="Preview"
                className="mt-4 w-32 h-32 object-cover rounded-full"
              />
            )}

            <p className="text-red-500 text-sm mt-1">{errors.profileImage}</p>
          </div>
        /* <div>
          <label className="block font-semibold">Profile Image</label>
          <input
            type="file"
            name="profileImage"
            accept="image/*"
            onChange={handleChange}
            className="w-full"
          />
          {imagePreview && (
            <img
              src={imagePreview}
              alt="Preview"
              className="mt-2 w-32 h-32 object-cover rounded-full"
            />
          )}
          <p className="text-red-500 text-sm mt-1">{errors.profileImage}</p>
        </div> */}

        {/* Full Name */}
        <div>
          <label className="block font-semibold">Full Name</label>
          <input
            name="fullName"
            className="w-full p-2 border rounded-md"
            value={formData.fullName}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.fullName}</p>
        </div>

        {/* Gender */}
        <div>
          <label className="block font-semibold">Gender</label>
          <select
            name="gender"
            className="w-full p-2 border rounded-md"
            value={formData.gender}
            onChange={handleChange}
          >
            <option value="">Select gender</option>
            <option value="Male">Male</option>
            <option value="Female">Female</option>
          </select>
          <p className="text-red-500 text-sm mt-1">{errors.gender}</p>
        </div>

        {/* Country */}
        <div>
          <label className="block font-semibold">Country</label>
          <input
            name="country"
            className="w-full p-2 border rounded-md"
            value={formData.country}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.country}</p>
        </div>

        {/* City */}
        <div>
          <label className="block font-semibold">City</label>
          <input
            name="city"
            className="w-full p-2 border rounded-md"
            value={formData.city}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.city}</p>
        </div>

        {/* Address */}
        <div>
          <label className="block font-semibold">Address</label>
          <input
            name="address"
            className="w-full p-2 border rounded-md"
            value={formData.address}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.address}</p>
        </div>

        {/* Email */}
        <div>
          <label className="block font-semibold">Email</label>
          <input
            name="email"
            type="email"
            className="w-full p-2 border rounded-md"
            value={formData.email}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.email}</p>
        </div>

        {/* Password */}
        <div>
          <label className="block font-semibold">Password</label>
          <input
            name="password"
            type="password"
            className="w-full p-2 border rounded-md"
            value={formData.password}
            onChange={handleChange}
          />
          <p className="text-red-500 text-sm mt-1">{errors.password}</p>
        </div>

        <button
          type="submit"
          className="w-full bg-blue-600 text-white p-2 rounded-md hover:bg-blue-700 transition"
        >
          Register
        </button>

        {message.text && (
          <p
            className={`text-sm mt-4 text-center ${message.type === "success" ? "text-green-600" : "text-red-600"
              }`}
          >
            {message.text}
          </p>
        )}
      </form>
    </div>
  );
};

export default Register;
