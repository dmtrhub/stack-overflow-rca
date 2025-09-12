import React, { useEffect, useState } from "react";
import {
  getAllQuestions,
  createQuestion,
} from "../services/QuestionService";
import { getToken } from "../services/AuthService";
import { Link } from "react-router-dom";

const Questions = () => {
  const [questions, setQuestions] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    title: "",
    description: "",
    questionImage: null,
  });
  const [preview, setPreview] = useState(null);
  const [errors, setErrors] = useState({});
  const [searchTitle, setSearchTitle] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  useEffect(() => {
    fetchQuestions();
  }, []);

  const fetchQuestions = async () => {
    const data = await getAllQuestions();
    setQuestions(data);
  };

  const handleToggleForm = () => {
    setShowForm(!showForm);
    setErrors({});
  };

  const searchQuestions = async () => {
    let url = `${import.meta.env.VITE_BACKEND_API_URL}/questions/search?`;
    if (searchTitle) url += `title=${encodeURIComponent(searchTitle)}&`;
    if (fromDate) url += `from=${fromDate}&`;
    if (toDate) url += `to=${toDate}&`;

    const res = await fetch(url, {
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    });

    if (res.ok) {
      const data = await res.json();
      setQuestions(data);
    }
  };

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

  const validateForm = () => {
    const newErrors = {};
    if (!formData.title.trim()) newErrors.title = "Title is required.";
    if (!formData.description.trim())
      newErrors.description = "Description is required.";
    if (!formData.questionImage) newErrors.questionImage = "Image is required.";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    const success = await createQuestion(formData);
    if (success) {
      setFormData({ title: "", description: "", questionImage: null });
      setPreview(null);
      setErrors({});
      setShowForm(false);
      fetchQuestions();
    } else {
      setErrors({ submit: "An error occurred while submitting the question." });
    }
  };

  return (
    <div className="p-4 max-w-2xl mx-auto">
      <button
        onClick={handleToggleForm}
        className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
      >
        Ask Question
      </button>

      {showForm && (
        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <input
              type="text"
              name="title"
              placeholder="Title"
              value={formData.title}
              onChange={handleChange}
              className="w-full border px-3 py-2 rounded"
            />
            {errors.title && (
              <p className="text-red-600 text-sm">{errors.title}</p>
            )}
          </div>

          <div>
            <textarea
              name="description"
              placeholder="Description"
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
              htmlFor="questionImage"
              className="inline-block cursor-pointer bg-gray-100 border border-gray-300 px-4 py-2 rounded hover:bg-gray-200"
            >
              Choose an image
            </label>
            <input
              id="questionImage"
              type="file"
              name="questionImage"
              accept="image/*"
              onChange={handleChange}
              className="hidden"
            />
            {formData.questionImage && (
              <p className="text-sm mt-1 text-gray-700">
                Selected: {formData.questionImage.name}
              </p>
            )}
            {errors.questionImage && (
              <p className="text-red-600 text-sm mt-1">
                {errors.questionImage}
              </p>
            )}
          </div>

          {preview && (
            <img
              src={preview}
              alt="Preview"
              className="w-32 h-32 object-cover border"
            />
          )}

          <button
            type="submit"
            className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
          >
            Submit
          </button>

          {errors.submit && (
            <p className="text-red-600 mt-2">{errors.submit}</p>
          )}
        </form>
      )}

      <div className="mt-6 flex flex-col gap-2 sm:flex-row sm:items-end sm:gap-4">
        <input
          type="text"
          placeholder="Search by title..."
          value={searchTitle}
          onChange={(e) => setSearchTitle(e.target.value)}
          className="border px-3 py-2 rounded w-full sm:w-1/3"
        />
        <input
          type="date"
          value={fromDate}
          onChange={(e) => setFromDate(e.target.value)}
          className="border px-3 py-2 rounded"
        />
        <input
          type="date"
          value={toDate}
          onChange={(e) => setToDate(e.target.value)}
          className="border px-3 py-2 rounded"
        />
        <button
          onClick={searchQuestions}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Search
        </button>
      </div>

      <div className="mt-6 space-y-6">
        {questions.map((question) => (
          <div
            key={question.RowKey}
            className="border p-4 rounded shadow-md bg-white"
          >
            <h2 className="text-xl font-semibold">{question.Title}</h2>
            <p className="mt-1 text-gray-700">{question.Description}</p>

            {question.PictureUrl && (
              <img
                src={question.PictureUrl}
                alt="Question"
                className="mt-2 w-full max-h-64 object-cover rounded"
              />
            )}

            <Link
              to={`/question/${question.Id}/answers`}
              className="mt-4 inline-block text-blue-600 hover:underline"
            >
              Answers
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Questions;
