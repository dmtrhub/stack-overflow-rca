import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuestionById, updateQuestion, } from "../services/QuestionService";

import { getUserIdFromToken } from "../services/AuthService";

const ModifyQuestion = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const userId = getUserIdFromToken();

    const [formData, setFormData] = useState({
        title: "",
        description: "",
        questionImage: null,
    });
    const [preview, setPreview] = useState(null);
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchQuestion = async () => {
            setLoading(true);
            const data = await getQuestionById(id);
            if (data) {
                setFormData({
                    title: data.Title || "",
                    description: data.Description || "",
                    questionImage: null,
                });
                setPreview(data.PictureUrl || null);
            }
            setLoading(false);
        };
        fetchQuestion();
    }, [id]);

    const handleChange = (e) => {
        const { name, value, files } = e.target;
        if (name === "questionImage") {
            const file = files[0];
            setFormData((prev) => ({ ...prev, questionImage: file }));
            setPreview(file ? URL.createObjectURL(file) : null);
        } else {
            setFormData((prev) => ({ ...prev, [name]: value }));
        }
    };

    const validate = () => {
        const newErrors = {};
        if (!formData.title.trim()) newErrors.title = "Title is required.";
        if (!formData.description.trim())
            newErrors.description = "Description is required.";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validate()) return;

        const result = await updateQuestion(id, formData);

        if (result.success) {
            navigate(`/profile/${userId}`);
        } else {
            const backendErrors = {};
            if (result.errors && result.errors.length) {
                result.errors.forEach((err) => {
                    backendErrors[err.field] = err.message;
                });
            }
            setErrors(backendErrors);
        }
    };

    if (loading) return <p>Loading question data...</p>;

    return (
        <div className="max-w-2xl mt-32 mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Edit Question</h1>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block mb-1 font-semibold">Title</label>
                    <input
                        type="text"
                        name="title"
                        value={formData.title}
                        onChange={handleChange}
                        className="w-full border px-3 py-2 rounded"
                    />
                    {errors.title && (
                        <p className="text-red-600 text-sm">{errors.title}</p>
                    )}
                </div>

                <div>
                    <label className="block mb-1 font-semibold">Description</label>
                    <textarea
                        name="description"
                        value={formData.description}
                        onChange={handleChange}
                        className="w-full border px-3 py-2 rounded"
                    />
                    {errors.description && (
                        <p className="text-red-600 text-sm">{errors.description}</p>
                    )}
                </div>

                <div>
                    <label
                        htmlFor="questionImage" // <--- ovo je ključno
                        className="block mb-1 font-semibold cursor-pointer bg-gray-100 border border-gray-300 px-4 py-2 rounded hover:bg-gray-200"
                    >
                        Choose new image (optional)
                    </label>
                    <input
                        type="file"
                        name="questionImage"
                        accept="image/*"
                        onChange={handleChange}
                        className="hidden"
                        id="questionImage"
                    />
                    {formData.questionImage && (
                        <p className="text-sm mt-1 text-gray-700">
                            Selected: {formData.questionImage.name}
                        </p>
                    )}
                    {preview && !formData.questionImage && (
                        <img
                            src={preview}
                            alt="Current"
                            className="mt-2 w-48 h-48 object-cover rounded"
                        />
                    )}
                    {preview && formData.questionImage && (
                        <img
                            src={preview}
                            alt="Preview"
                            className="mt-2 w-48 h-48 object-cover rounded"
                        />
                    )}
                </div>

                <button
                    type="submit"
                    className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700"
                >
                    Update Question
                </button>
            </form>
        </div>
    );
};

export default ModifyQuestion;
