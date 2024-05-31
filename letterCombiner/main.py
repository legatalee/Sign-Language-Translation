from util import join_jamos
import re

def split(query):
    return query.replace(' ', '').split(',')

def assemble(letter_list):
    return join_jamos(letter_list)
    
def removeSingLetter(word):
    return re.sub(r'[ㄱ-ㅎㅏ-ㅣ]', '', word)

def lambda_handler(event, context):
    print(event)
    query = event['queryStringParameters']['query']
    print('query', query)
    letter_list = split(query)
    print('letter_list', letter_list)
    result = removeSingLetter(assemble(letter_list))
    print('result', result)
    return {
        'statusCode': 200,
        'body': result
    }