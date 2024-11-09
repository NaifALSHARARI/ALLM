from camel_tools.morphology.analyzer import Analyzer
from camel_tools.morphology.database import MorphologyDB
import sys
import json
import io
import re

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def tokenize(text):
    text = re.sub(r'[^\w\s]', ' ', text)
    words = text.split()
    return words

def is_feminine_name(word):
    feminine_names = {"سارة", "نورة", "فاطمة","وريم","ريم" ,"مريم", "هند", "ليلى", "أسماء", "عائشة", "زينب"}
    return word in feminine_names or word.endswith("ة")

def analyze_text(text):
    db = MorphologyDB.builtin_db()
    analyzer = Analyzer(db)
    words = tokenize(text)
    result = []

    for word in words:
        analyses = analyzer.analyze(word)
        word_info = {'word': word}
        
        if analyses:
            analysis = analyses[0]

            gen = analysis.get('gen')
            if gen == 'f' or is_feminine_name(word):
                word_info['gender'] = 'feminine'
            elif gen == 'm':
                word_info['gender'] = 'masculine'
            else:
                word_info['gender'] = 'unknown'

            pos = analysis.get('pos')
            word_info['pos'] = pos

            word_info['pronoun'] = pos == 'pron'

            if pos == 'verb':
                aspect = analysis.get('asp')
                prc0 = analysis.get('prc0', '')
                prc1 = analysis.get('prc1', '')

                if prc0 in ['sa', 'sawfa'] or prc1 in ['sa', 'sawfa']:
                    word_info['tense'] = 'future'
                elif aspect == 'p':
                    word_info['tense'] = 'past'
                elif aspect == 'i':
                    word_info['tense'] = 'present'
                else:
                    word_info['tense'] = 'unknown'
            else:
                word_info['tense'] = 'unknown'
        else:
            if is_feminine_name(word):
                word_info['gender'] = 'feminine'
            else:
                word_info['gender'] = 'unknown'

            word_info.update({'pos': 'unknown', 'pronoun': False, 'tense': 'unknown'})

        result.append(word_info)

    return result

if __name__ == "__main__":
    if len(sys.argv) > 1:
        input_file = sys.argv[1]
        with open(input_file, 'r', encoding='utf-8') as f:
            text = f.read()
    else:
        text = ''
    analysis = analyze_text(text)
    print(json.dumps(analysis, ensure_ascii=False))
