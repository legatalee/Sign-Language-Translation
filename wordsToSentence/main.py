import openai
import os

openai.api_key=os.environ.get('api_key')

def split(query):
    return query.replace(' ', '').split(',')

def lambda_handler(event, context):
  print(event)
  query = event['queryStringParameters']['query']
  words = split(query)
  
  # words = event.get('words', [])

  words_list = [f"'{word}'" for word in words]
  words_list = ' '.join(words_list)
  
  prompt_text = '를 입력 순서대로 하나 또는 여러개의 문장으로 만들어줘. 동사는 한 문장에 하나씩 들어갈 수 있도록 해. 최대한 간단하게 문장으로 만들어줘. 하나의 단어는 한 번만 사용해줘. 주어진 단어 외에 너무 많은 의미 첨가는 하지마. 한국어 어순을 지켜줘. 공식적인 자리에 쓰는 대화라고 생각하고 경어체로 만들어줘.'
  # prompt_text = '를 입력 순서대로 하나의 문장으로 만들어줘. 자연스럽게 사람이 알아들을 수 있는 문장으로 만들어줘야 돼'
  

  response = openai.ChatCompletion.create(
    model="gpt-4o",
    messages= [{"role": "user" ,"content" : words_list + " " + prompt_text}],
    temperature=0.7,
    max_tokens=100,
    top_p=1.0,
    frequency_penalty=0.0,
    presence_penalty=0.0
  )
  
  print(words_list + " " + prompt_text)
  
  response_text = response['choices'][0]['message']['content'].strip()
  print(response_text)

  # TODO implement
  return {
      'statusCode': 200,
      'body': response_text
  }

