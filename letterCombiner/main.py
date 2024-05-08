from util import join_jamos

def split(query):
    return query.split(',')

def assemble(letter_list):
    return join_jamos(['ㅇ', 'ㅔ', 'ㄱ', 'ㅡ', 'ㄷ', 'ㅡ','ㄹ', 'ㅏ', 'ㅂ'])

def handler(event, context):
    
    query = event['queryStringParameters']['query']

    letter_list = split(query)
    result = assemble(letter_list)
    
    return {
        'statusCode': 200,
        'result': result
    }