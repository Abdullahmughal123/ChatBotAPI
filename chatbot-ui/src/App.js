import { useState, useRef, useEffect } from "react";
import axios from "axios";
import "./styles.css";

function App() {
  const [message, setMessage] = useState("");
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [historyLoading, setHistoryLoading] = useState(false);
  const chatEndRef = useRef(null);

  // Auto-scroll to latest message
  const scrollToBottom = () => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  // Load chat history on component mount
  useEffect(() => {
    loadChatHistory();
  }, []);

  // Auto-scroll when messages change
  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // Load chat history from API
  const loadChatHistory = async () => {
    try {
      setHistoryLoading(true);
      console.log("Loading chat history...");
      
      const res = await axios.get("http://localhost:5274/api/chat/history?page=1&pageSize=50");
      
      console.log("Chat History Response:", res.data);
      
      // Map API response to message format - interleave user and bot messages
      const historyData = res.data?.data || [];
      const historyMessages = [];
      
      historyData.forEach(msg => {
        // Add user message
        historyMessages.push({
          text: msg.userMessage,
          sender: "user"
        });
        
        // Add bot response
        historyMessages.push({
          text: msg.botResponse,
          sender: "bot"
        });
      });

      setMessages(historyMessages || []);
      console.log("Loaded", historyMessages?.length || 0, "messages from history");
      
    } catch (err) {
      console.error("Error loading chat history:", err);
      // Don't show error to user on initial load, just start with empty chat
    } finally {
      setHistoryLoading(false);
    }
  };

  const sendMessage = async () => {
    if (!message.trim() || loading) return;

    const userMsg = message.trim();

    // Add user message instantly to UI
    setMessages(prev => [...prev, { sender: "user", text: userMsg }]);
    
    // Clear input and set loading state
    setMessage("");
    setLoading(true);

    try {
      console.log("Sending message:", userMsg);
      
      const res = await axios.post(
        "http://localhost:5274/api/chat",
        { message: userMsg },
        {
          headers: { "Content-Type": "application/json" },
        }
      );

      console.log("POST API Response:", res.data);

      // Extract bot response from correct API structure
      const botReply = res.data?.data?.response || res.data?.data?.reply || "I'm sorry, I couldn't process that.";
      
      // Add bot response to chat
      setMessages(prev => [...prev, { sender: "bot", text: botReply }]);
      
    } catch (err) {
      console.error("POST API Error:", err);
      
      // Extract error message from API response if available
      const errorMessage = err.response?.data?.message || 
                           err.response?.data?.errors?.[0] || 
                           "Something went wrong. Please try again.";
      
      setMessages(prev => [...prev, { sender: "bot", text: errorMessage }]);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === "Enter" && !loading && message.trim()) {
      sendMessage();
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h2>Chatbot</h2>
        <button 
          onClick={loadChatHistory} 
          style={styles.refreshButton}
          disabled={historyLoading}
        >
          {historyLoading ? "..." : "Refresh"}
        </button>
      </div>

      <div style={styles.chatBox}>
        {/* Show loading indicator for history */}
        {historyLoading && (
          <div style={{ textAlign: "center", padding: "20px", color: "#666" }}>
            Loading chat history...
          </div>
        )}

        {/* Render messages */}
        {messages.map((c, i) => (
          <div
            key={i}
            style={{
              ...styles.message,
              alignSelf: c.sender === "user" ? "flex-end" : "flex-start",
              backgroundColor: c.sender === "user" ? "#007bff" : "#e5e5e5",
              color: c.sender === "user" ? "white" : "black",
            }}
          >
            <span style={styles.messageSender}>
              {c.sender === "user" ? "You" : "Bot"}
            </span>
            {c.text}
          </div>
        ))}

        {/* Typing indicator */}
        {loading && (
          <div style={{ ...styles.message, alignSelf: "flex-start", backgroundColor: "#e5e5e5", color: "black" }}>
            <span style={styles.messageSender}>Bot</span>
            <div className="typing-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        )}
        
        {/* Auto-scroll anchor */}
        <div ref={chatEndRef} />
      </div>

      <div style={styles.inputBox}>
        <input
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Type message..."
          style={styles.input}
          disabled={loading}
        />
        <button 
          onClick={sendMessage} 
          style={{ 
            ...styles.button, 
            backgroundColor: loading || !message.trim() ? "#ccc" : "#28a745",
            cursor: loading || !message.trim() ? "not-allowed" : "pointer"
          }}
          disabled={loading || !message.trim()}
        >
          {loading ? "..." : "Send"}
        </button>
      </div>
    </div>
  );
}

const styles = {
  container: {
    width: "450px",
    margin: "30px auto",
    fontFamily: "Arial, sans-serif",
    boxShadow: "0 4px 8px rgba(0,0,0,0.1)",
    borderRadius: "10px",
    overflow: "hidden",
  },
  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "15px 20px",
    backgroundColor: "#fff",
    borderBottom: "1px solid #ddd",
  },
  refreshButton: {
    padding: "8px 15px",
    backgroundColor: "#007bff",
    color: "white",
    border: "none",
    borderRadius: "15px",
    cursor: "pointer",
    fontSize: "12px",
    transition: "all 0.3s",
  },
  "refreshButton:hover:not(:disabled)": {
    backgroundColor: "#0056b3",
  },
  "refreshButton:disabled": {
    backgroundColor: "#ccc",
    cursor: "not-allowed",
  },
  chatBox: {
    height: "450px",
    border: "1px solid #ddd",
    padding: "15px",
    display: "flex",
    flexDirection: "column",
    overflowY: "auto",
    backgroundColor: "#f9f9f9",
  },
  message: {
    padding: "12px 15px",
    margin: "8px 0",
    borderRadius: "18px",
    maxWidth: "75%",
    wordWrap: "break-word",
    lineHeight: "1.4",
  },
  messageSender: {
    fontSize: "11px",
    fontWeight: "bold",
    opacity: 0.7,
    marginBottom: "4px",
    display: "block",
  },
  typingIndicator: {
    display: "flex",
    gap: "4px",
    padding: "8px 0",
  },
  inputBox: {
    display: "flex",
    padding: "15px",
    backgroundColor: "#fff",
    borderTop: "1px solid #ddd",
    gap: "10px",
  },
  input: {
    flex: 1,
    padding: "12px 15px",
    border: "1px solid #ddd",
    borderRadius: "25px",
    fontSize: "14px",
    outline: "none",
    transition: "border-color 0.3s",
  },
  "input:focus": {
    borderColor: "#007bff",
  },
  "input:disabled": {
    backgroundColor: "#f5f5f5",
    cursor: "not-allowed",
  },
  button: {
    padding: "12px 20px",
    backgroundColor: "#28a745",
    color: "white",
    border: "none",
    borderRadius: "25px",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "bold",
    transition: "all 0.3s",
    minWidth: "80px",
  },
};

export default App;