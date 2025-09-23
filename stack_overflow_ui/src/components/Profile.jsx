import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getMyQuestions, deleteQuestion } from "../services/QuestionService";

const Profile = () => {
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    fetchMyQuestions();
  }, []);

  const fetchMyQuestions = async () => {
    setLoading(true);
    const data = await getMyQuestions();
    setQuestions(data);
    setLoading(false);
  };

  const handleDelete = async (id) => {
    if (window.confirm("Are you sure you want to delete this question?")) {
      const success = await deleteQuestion(id);
      if (success) {
        setQuestions((prev) => prev.filter((q) => q.Id !== id));
      } else {
        alert("Failed to delete question.");
      }
    }
  };

  const handleEdit = (id) => {
    navigate(`/question/${id}/modify`);
  };

  return (
    <div className="max-w-4xl mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">My Questions</h1>

      {loading ? (
        <p>Loading your questions...</p>
      ) : questions.length === 0 ? (
        <p>You have no questions yet.</p>
      ) : (
        <div className="space-y-6">
          {questions.map((q) => (
            <div
              key={q.Id}
              className="border rounded p-4 shadow-sm bg-white flex flex-col md:flex-row md:justify-between md:items-center"
            >
              <div>
                <h3 className="text-lg font-semibold">{q.Title}</h3>
                <p className="text-gray-700 mt-1">{q.Description}</p>
                {q.PictureUrl && (
                  <img
                    src={q.PictureUrl}
                    alt="Question"
                    className="mt-2 max-h-48 rounded object-cover"
                  />
                )}
              </div>

              <div className="mt-4 md:mt-0 space-x-2 flex">
                <button
                  onClick={() => handleEdit(q.Id)}
                  className="bg-yellow-400 hover:bg-yellow-500 text-white px-4 py-2 rounded"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(q.Id)}
                  className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Profile;
