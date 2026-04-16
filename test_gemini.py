import requests
import json

# Test Gemini API directly
api_key = "AIzaSyDqGsfnqDdxjrapsMuR4lADr0SYevn7hPw"
url = f"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={api_key}"

payload = {
    "contents": [{
        "parts": [{
            "text": "Hello, how are you?"
        }]
    }]
}

try:
    response = requests.post(url, json=payload)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")
