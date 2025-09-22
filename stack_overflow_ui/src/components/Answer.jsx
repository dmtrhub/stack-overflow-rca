import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  getAnswersByQuestionId,
  createAnswer,
} from "../services/AnswerService";
import { getQuestionById, closeQuestion } from "../services/QuestionService";
import { getUserEmailFromToken } from "../services/AuthService";

import {
  voteForAnswer,
  unvoteAnswer,
  hasUserVoted,
} from "../services/VoteService";

import { faStar as solidStar } from "@fortawesome/free-solid-svg-icons";
import { faStar as regularStar } from "@fortawesome/free-regular-svg-icons";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faHeart as solidHeart } from "@fortawesome/free-solid-svg-icons";
import { faHeart as regularHeart } from "@fortawesome/free-regular-svg-icons";

const Answer = () => {
  const { id } = useParams();
  const [question, setQuestion] = useState(null);
  const [answers, setAnswers] = useState([]);
  const [error, setError] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [newAnswer, setNewAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [votedAnswers, setVotedAnswers] = useState({});
  const [sortByVotes, setSortByVotes] = useState(null);
  const loggedUserEmail = getUserEmailFromToken();

  const checkVotes = async (answers) => {
    const results = {};
    for (const answer of answers) {
      const voted = await hasUserVoted(answer.id);
      results[answer.id] = voted;
    }
    setVotedAnswers(results);
  };

  const handleVoteToggle = async (answerId) => {
    const alreadyVoted = votedAnswers[answerId];

    const success = alreadyVoted
      ? await unvoteAnswer(answerId)
      : await voteForAnswer(answerId);

    if (success) {
      fetchAnswers(); // refresh votes
    }
  };

  const fetchAnswers = async () => {
    try {
      const data = await getAnswersByQuestionId(id);
      const camelCased = data.map((a) => ({
        id: a.Id,
        questionId: a.QuestionId,
        description: a.Description,
        numberOfVotes: a.NumberOfVotes,
        authorEmail: a.AnsweredByEmail,
        createdAt: a.CreatedAt,
      }));
      let sorted = [...camelCased];

      if (sortByVotes === "asc") {
        sorted.sort((a, b) => a.numberOfVotes - b.numberOfVotes);
      } else if (sortByVotes === "desc") {
        sorted.sort((a, b) => b.numberOfVotes - a.numberOfVotes);
      } else {
        sorted.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
      }

      setAnswers(sorted);
      await checkVotes(sorted);
    } catch (err) {
      setError("Failed to load answers.");
    }
  };

  const fetchQuestion = async () => {
    try {
      const q = await getQuestionById(id);
      if (q) {
        setQuestion({
          id: q.Id,
          title: q.Title,
          description: q.Description,
          pictureUrl: q.PictureUrl,
          createdBy: q.CreatedBy,
          topAnswerId: q.TopAnswerId,
          isClosed: q.IsClosed,
          createdAt: q.CreatedAt,
          updatedAt: q.UpdatedAt,
        });
      }
    } catch (err) {
      setError("Failed to load question.");
    }
  };

  useEffect(() => {
    fetchQuestion();
    fetchAnswers();
  }, [id]);

  useEffect(() => {
    fetchAnswers();
  }, [sortByVotes]);

  const handleAnswerClick = () => {
    setShowForm(!showForm);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!newAnswer.trim()) return;

    setSubmitting(true);
    try {
      await createAnswer({ questionId: id, description: newAnswer });
      setNewAnswer("");
      setShowForm(false);
      fetchAnswers();
    } catch (err) {
      setError("Failed to submit answer.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleCloseQuestion = async (answerId) => {
    const success = await closeQuestion(question.id, answerId);
    if (success) {
      setQuestion({ ...question, isClosed: true, topAnswerId: answerId });
    }
  };

  return (
    <div className="mt-36 max-w-2xl mx-auto">
      {error && <p className="text-red-500">{error}</p>}

      {question && (
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">{question.title}</h1>
          <p className="mb-4 text-gray-700">{question.description}</p>
          {question.pictureUrl && (
            <img
              src={question.pictureUrl}
              alt="Question"
              className="rounded w-full max-h-96 object-contain mb-6"
            />
          )}
        </div>
      )}

      <button
        onClick={handleAnswerClick}
        disabled={question?.isClosed}
        className={`mb-6 px-4 py-2 text-white rounded ${
          question?.isClosed
            ? "bg-gray-400 cursor-not-allowed"
            : "bg-blue-600 hover:bg-blue-700"
        }`}
        title={question?.isClosed ? "Question is closed" : ""}
      >
        {showForm ? "Cancel" : "Answer"}
      </button>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6">
          <textarea
            className="w-full p-2 border rounded mb-2"
            rows="4"
            placeholder="Write your answer..."
            value={newAnswer}
            onChange={(e) => setNewAnswer(e.target.value)}
          ></textarea>
          <button
            type="submit"
            disabled={submitting}
            className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
          >
            {submitting ? "Submitting..." : "Submit"}
          </button>
        </form>
      )}

      {/* Sort Dropdown */}
      <div className="mb-4 flex gap-4 items-center">
        <label className="text-gray-700">Sort by votes:</label>
        <select
          value={sortByVotes || ""}
          onChange={(e) => setSortByVotes(e.target.value || null)}
          className="border px-3 py-1 rounded"
        >
          <option value="">Newest</option>
          <option value="asc">Least votes</option>
          <option value="desc">Most votes</option>
        </select>
      </div>

      {answers.map((answer) => (
        <div
          key={answer.id}
          className="mb-4 p-4 border rounded shadow flex justify-between items-start"
        >
          <div>
            <p>{answer.description}</p>
            <p className="text-sm text-gray-500">By: {answer.authorEmail}</p>
            <p className="text-xs text-gray-400">
              {new Date(answer.createdAt).toLocaleString()}
            </p>
          </div>
          <div className="flex flex-col items-center ml-4">
            <button
              onClick={() => handleVoteToggle(answer.id)}
              className="hover:text-red-600"
              disabled={question?.isClosed}
              title={votedAnswers[answer.id] ? "Unvote" : "Upvote"}
            >
              <FontAwesomeIcon
                icon={votedAnswers[answer.id] ? solidHeart : regularHeart}
                className={`text-2xl transition-colors ${
                  votedAnswers[answer.id] ? "text-red-500" : "text-gray-500"
                }`}
              />
            </button>
            <span className="text-sm mt-1">{answer.numberOfVotes}</span>

            {question &&
              question.createdBy === loggedUserEmail &&
              (!question.isClosed ? (
                <button
                  onClick={() => handleCloseQuestion(answer.id)}
                  className="mt-2 text-gray-500 hover:text-yellow-600 transition-colors"
                  title="Mark as Top Answer & Close Question"
                >
                  <FontAwesomeIcon icon={regularStar} className="text-xl" />
                </button>
              ) : (
                question.topAnswerId === answer.id && (
                  <span className="mt-2 text-yellow-500" title="Top Answer">
                    <FontAwesomeIcon icon={solidStar} className="text-xl" />
                  </span>
                )
              ))}
          </div>
        </div>
      ))}
    </div>
  );
};

export default Answer;
